// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace NativeClient
{
    /// <summary>
    /// This class allows to estimate the time difference between the client and the server. It uses multiple calls to
    /// remote server to estimate the latency and compensates time difference returned by server by this latency.
    /// </summary>
    class TimeDelayEstimator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NativeClient.TimeDelayEstimator"/> class.
        /// </summary>
        /// <param name="aCommunicator">The communicator connected to the server.</param>
        public TimeDelayEstimator(Communicator aCommunicator)
        {
            if (!aCommunicator.IsConnected)
                throw new ArgumentException("Communicator must be connected when passed to Authenticator constructor.");

            communicator = aCommunicator;
        }

        /// <summary>
        /// Occurs when time delay has been estimated.
        /// </summary>
        public event EventHandler<TimeDelayEventArgs> TimeDelayEstimated;

        /// <summary>
        /// Starts the estimating process. Performs "heat-up" of the network pipeline - first 100 samples are ignored
        /// as they will cause the virtual machine (Mono) to compile and optimize the networking code. Only later
        /// samples are used for estimating the latency. It is recommended to estimate the latency when no other heavy
        /// load on the connection is present as the latter may affect the estimates.
        /// </summary>
        /// <param name="numSamples">Number samples to estimate upon.</param>
        public void StartEstimating(int numSamples)
        {
            totalCalls = numSamples + heatUpCalls;
            numCalls = 0;
            accumulatedLatencyMs = new TimeSpan(0);
            callStart = DateTime.Now;
            int callID = communicator.Call("getTime");
            communicator.AddReplyHandler(callID, ProcessReturnedCall);
        }

        /// <summary>
        /// Process the completed call sample.
        /// </summary>
        /// <param name="reply">Call reply arguments.</param>
        void ProcessReturnedCall(CallReply reply)
        {
            DateTime callEnd = DateTime.Now;

            numCalls++;

            // Only collect samples that are not in the heat-up process.
            if (numCalls > heatUpCalls)
                accumulatedLatencyMs += (callEnd - callStart);

            if (numCalls < totalCalls)
            {
                // Start next call.
                callStart = DateTime.Now;
                int callID = communicator.Call("getTime");
                communicator.AddReplyHandler(callID, ProcessReturnedCall);
            }
            else if (TimeDelayEstimated != null)
            {
                // Estimate latency and compute time delay.
                double latency = accumulatedLatencyMs.TotalMilliseconds / totalCalls;
                DateTime remoteTime = reply.RetValue.ToObject<DateTime>();
                TimeSpan timeDifference = remoteTime - callStart;
                double timeDelay = timeDifference.TotalMilliseconds - latency / 2;
                TimeDelayEstimated(this, new TimeDelayEventArgs(timeDelay));
            }
        }

        Communicator communicator;

        DateTime callStart;
        TimeSpan accumulatedLatencyMs;
        int numCalls;
        int totalCalls;
        int heatUpCalls = 100;
    }
}

