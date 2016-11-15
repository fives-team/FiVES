using SINFONI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientManagerPlugin
{
    class ClientQueue
    {
        private ClientFunction Callback { get; set; }
        private List<UpdateInfo> Queue { get; set; }
        private string[] InterestedIn;
        private CancellationTokenSource CancellationSource;
        bool Running = true;
        bool Sending = false;
        public ClientQueue(ClientFunction callback, string[] interestedIn)
        {
            InterestedIn = interestedIn;
            Callback = callback;
            Queue = new List<UpdateInfo>();
            CancellationSource = new CancellationTokenSource();
            var cancellationToken = CancellationSource.Token;
            Task.Factory.StartNew(FlushClientQueue, cancellationToken)
                .ContinueWith(t => { Console.WriteLine("Update Task was aborted gracefully from outside: {0}"); },
                TaskContinuationOptions.OnlyOnCanceled)
                .ContinueWith(t => { Console.WriteLine("Update Task Aborted unexpectedly: {0}", t.Exception); }
                    , TaskContinuationOptions.OnlyOnFaulted);
        }

        public ClientQueue(ClientFunction callback) : this(callback, new string[0]) { }

        public void AddUpdates(UpdateInfo[] globalUpdates)
        {
            bool gotLock = false;
            try
            {
                queueLock.Enter(ref gotLock);
                copyGlobalUpdatesToLocal(globalUpdates);
            }
            finally
            {
                if (gotLock)
                    queueLock.Exit();
            }
        }

        public void Abort()
        {
            CancellationSource.Cancel();
            Running = false;
        }

        private void copyGlobalUpdatesToLocal(UpdateInfo[] globalUpdates)
        {
            foreach (UpdateInfo u in globalUpdates)
            {
                if (InterestedIn.Length > 0 && !InterestedIn.Contains(u.componentName))
                    continue;
                Queue.Add(u);
            }
        }

        private bool IsAlreadyInQueue(UpdateInfo newUpdate, ref UpdateInfo existingUpdate)
        {
            Predicate<UpdateInfo> predicate = new Predicate<UpdateInfo>(u => u.entityGuid == newUpdate.entityGuid
                && u.componentName.Equals(newUpdate.componentName)
                && u.attributeName.Equals(newUpdate.componentName));

            bool exists = Queue.Any(u => predicate(u));

            if (exists)
            {
                // existingUpdate = Queue.Find(predicate);
            }

            return exists;
        }

        private void FlushClientQueue()
        {
            while (true)
            {
                bool gotLock = false;
                try
                {
                    queueLock.Enter(ref gotLock);
                    UpdateInfo[] queueSnapshot = new UpdateInfo[Queue.Count];
                    Queue.CopyTo(queueSnapshot);
                    Callback(queueSnapshot);
                }
                finally
                {
                    if (gotLock)
                        queueLock.Exit();
                }
                Thread.Sleep(15);
            }
        }

        SpinLock queueLock = new SpinLock();
    }
}
