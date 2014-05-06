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

namespace KIARAPlugin
{
    /// <summary>
    /// Exception that is thrown when waiting for the completion of the call has exceeded the timeout value.
    /// </summary>
    public class TimeoutException : Exception
    {
        public TimeoutException() : base() { }
        public TimeoutException(string message) : base(message) { }
    }

    /// <summary>
    /// Represents an initiated call to the remote function. May be used to set up handlers for various outcomes and/or
    /// wait for the completion of the call.
    /// </summary>
    public interface IFuncCall
    {
        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when the call completes successfully. The return value is
        /// converted into <typeparamref name="T" /> and passed as a single argument to the <paramref name="handler"/>.
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed upon successful completion of the call.</param>
        /// <typeparam name="T">Type to which return value should be converted.</typeparam>
        IFuncCall OnSuccess<T>(Action<T> handler);

        /// <summary>
        /// Same as <see cref="onSuccess<T>"/> except that returned value (if any) is ignored and the handler is called
        /// with no parameters.
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed upon successful completion of the call.</param>
        IFuncCall OnSuccess(Action handler);

        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when an exception was thrown from the call. Exception is
        /// passed as a first argument to the <paramref name="handler"/>
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed when exception is thrown.</param>
        IFuncCall OnException(Action<Exception> handler);

        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when an error happened during the call. The reason for the
        /// error is passed as a first argument to the <paramref name="handler"/>. When the error is reported the
        /// outcome of the call on the remote end is unknown.
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed when error happened during the call.</param>
        IFuncCall OnError(Action<string> handler);

        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when the an error happened during the call or an exception 
        /// is thrown.
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed upon successful completion of the call.</param>
        IFuncCall OnFailure(Action handler);

        /// <summary>
        /// Executes the call syncrhonously. Converts a value returned from the call into type <typeparamref name="T">
        /// and returns it. On error a <see cref="KIARA.Error"/> exception is raised. Remote exceptions are raised 
        /// locally. All assigned handlers for this call are executed before returning from this call. Times out after
        /// <paramref name="millisecondsTimeout"/> and throws <see cref="TimeoutException"/>. If 
        /// <paramref name="millisecondsTimeout"/> is -1 (default value), then the methods waits indefinitely.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds.</param>
        /// <typeparam name="T">Type to which returned value should be converted.</typeparam>
        T Wait<T>(int millisecondsTimeout = -1);

        // Executes the call syncrhonously. Return value (if any) is ignored. On error a KIARA.Error exception is
        // raised. Remote exceptions are raised locally. All assigned handlers for this call are executed before
        // returning from this call. Times out after |millisecondsTimeout| and throws TimeoutException or waits
        // indefinitely if the value is -1.

        /// <summary>
        /// Same as <see cref="wait<T>"/> except that returned value (if any) is ignored.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds.</param>
        void Wait(int millisecondsTimeout = -1);
    }
}

