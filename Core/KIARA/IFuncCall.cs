using System;

namespace KIARA
{
    /// <summary>
    /// Exception that is thrown when waiting for the completion of the call has exceeded the timeout value.
    /// </summary>
    public class TimeoutException : Exception
    {
        public TimeoutException() : base() { }
        public TimeoutException(string message) : base(message) { }
    }

    // Represents an initiated call to the remote function. May be used to set up handlers for various outcomes and/or
    // wait for the completion of the call.
    public interface IFuncCall
    {
        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when the call completes successfully. The return value is
        /// converted into <typeparamref name="T" /> and passed as a single argument to the <paramref name="handler"/>.
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed upon successful completion of the call.</param>
        /// <typeparam name="T">Type to which return value should be converted.</typeparam>
        IFuncCall onSuccess<T>(Action<T> handler);

        /// <summary>
        /// Same as <see cref="onSuccess<T>"/> except that returned value (if any) is ignored and the handler is called
        /// with no parameters.
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed upon successful completion of the call.</param>
        IFuncCall onSuccess(Action handler);

        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when an exception was thrown from the call. Exception is
        /// passed as a first argument to the <paramref name="handler"/>
        /// </summary>
        /// <returns>This call object.</returns>
        /// <param name="handler">Handler to be executed when exception is thrown.</param>
        IFuncCall onException(Action<Exception> handler);

        /// <summary>
        /// Adds a <paramref name="handler"/> to be called when an error happened during the call. The reason for the
        /// error is passed as a first argument to the <paramref name="handler"/>. When the error is reported the
        /// outcome of the call on the remote end is unknown.
        /// </summary>
        /// <returns>The error.</returns>
        /// <param name="handler">Handler.</param>
        IFuncCall onError(Action<string> handler);

        /// <summary>
        /// Executes the call syncrhonously. Converts a value returned from the call into type <typeparamref name="T"> 
        /// and returns it. On error a <see cref="KIARA.Error"/> exception is raised. Remote exceptions are raised 
        /// locally. All assigned handlers for this call are executed before returning from this call. Times out after
        /// <paramref name="millisecondsTimeout"/> and throws <see cref="TimeoutException"/>. If 
        /// <paramref name="millisecondsTimeout"/> is -1 (default value), then the methods waits indefinitely.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds.</param>
        /// <typeparam name="T">Type to which returned value should be converted.</typeparam>
        T wait<T>(int millisecondsTimeout = -1);

        // Executes the call syncrhonously. Return value (if any) is ignored. On error a KIARA.Error exception is
        // raised. Remote exceptions are raised locally. All assigned handlers for this call are executed before
        // returning from this call. Times out after |millisecondsTimeout| and throws TimeoutException or waits
        // indefinitely if the value is -1.

        /// <summary>
        /// Same as <see cref="wait<T>"/> except that returned value (if any) is ignored.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds.</param>
        void wait(int millisecondsTimeout = -1);
    }
}

