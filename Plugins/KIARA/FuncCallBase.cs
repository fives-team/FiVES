using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace KIARAPlugin
{
    /// <summary>
    /// An base implementation of the <see cref="IFuncCall"/>, which allows many protocols to reuse the same code.
    /// </summary>
    public abstract class FuncCallBase : IFuncCall
    {
        #region IFuncCall implementation

        public IFuncCall OnSuccess<T>(Action<T> handler)
        {
            if (state == State.InProgress)
                successHandlers.Add(handler);
            else if (state == State.Success)
                handler((T)ConvertResult(cachedResult, typeof(T)));
            return this;
        }

        public IFuncCall OnSuccess(Action handler)
        {
            if (state == State.InProgress)
                successHandlers.Add(handler);
            else if (state == State.Success)
                handler();
            return this;
        }

        public IFuncCall OnException(Action<Exception> handler)
        {
            if (state == State.InProgress)
                exceptionHandlers.Add(handler);
            else if (state == State.Exception)
                handler((Exception)cachedResult);
            return this;
        }

        public IFuncCall OnError(Action<string> handler)
        {
            if (state == State.InProgress)
                errorHandlers.Add(handler);
            else if (state == State.Error)
                handler((string)cachedResult);
            return this;
        }

        public IFuncCall OnFailure(Action handler)
        {
            if (state == State.InProgress)
            {
                errorHandlers.Add((reason) => handler());
                exceptionHandlers.Add((exception) => handler());
            }
            else if (state == State.Error || state == State.Exception)
            {
                handler();
            }
            return this;
        }

        public T Wait<T>(int millisecondsTimeout = -1)
        {
            T result = default(T);
            OnSuccess<T>(delegate (T value) { result = value; });
            Wait(millisecondsTimeout);
            return result;
        }

        public void Wait(int millisecondsTimeout = -1)
        {
            if (millisecondsTimeout == -1)
                callFinished.WaitOne();
            else
                callFinished.WaitOne(millisecondsTimeout);

            if (state == State.Error)
                throw new Error(ErrorCode.CONNECTION_ERROR, "Error during the call. Reason: " + (string)cachedResult);
            else if (state == State.Exception)
                throw (Exception)cachedResult;
            else if (state == State.InProgress)
                throw new TimeoutException("Call timed out after " + millisecondsTimeout + "ms");
        }

        #endregion

        /// <summary>
        /// Notifies the clients of this call that the call was completed successfully. The <paramref name="retValue"/>
        /// is passed into success handlers.
        /// </summary>
        /// <param name="retValue">Value returned by the call.</param>
        public virtual void HandleSuccess(object retValue)
        {
            state = State.Success;
            cachedResult = retValue;

            foreach (var handler in successHandlers) {
                Debug.Assert(handler.Method.GetParameters().Length <= 1);
                if (handler.Method.GetParameters().Length == 0) {
                    handler.DynamicInvoke();
                } else {
                    Type argType = handler.Method.GetParameters()[0].GetType();
                    handler.DynamicInvoke(ConvertResult(retValue, argType));
                }
            }

            successHandlers.Clear();
            callFinished.Set();
        }

        /// <summary>
        /// Notifies the clients of this call that an exception was thrown from the call. The
        /// <paramref name="exception"/> is passed into exception handlers.
        /// </summary>
        /// <param name="exception">Exception that was thrown.</param>
        public virtual void HandleException(Exception exception)
        {
            state = State.Exception;
            cachedResult = exception;

            foreach (var handler in exceptionHandlers)
                handler(exception);

            exceptionHandlers.Clear();
            callFinished.Set();
        }

        /// <summary>
        /// Notifies the clients of this call that an error has occured during the call. The <paramref name="reason"/>
        /// is passed into error handlers.
        /// </summary>
        /// <param name="reason">The reason for the error.</param>
        public virtual void HandleError(string reason)
        {
            state = State.Error;
            cachedResult = reason;

            foreach (var handler in errorHandlers)
                handler(reason);

            errorHandlers.Clear();
            callFinished.Set();
        }

        /// <summary>
        /// Converts the <paramref name="result"/> into <paramref name="type"/>.
        /// </summary>
        /// <returns>The converted result.</returns>
        /// <param name="result">Result.</param>
        /// <param name="type">Type to which the result must be converted.</param>
        protected abstract object ConvertResult(object result, Type type);

        /// <summary>
        /// The registered success handlers.
        /// </summary>
        protected List<Delegate> successHandlers = new List<Delegate>();

        /// <summary>
        /// The registered exception handlers.
        /// </summary>
        protected List<Action<Exception>> exceptionHandlers = new List<Action<Exception>>();

        /// <summary>
        /// The registered error handlers.
        /// </summary>
        protected List<Action<string>> errorHandlers = new List<Action<string>>();

        /// <summary>
        /// The event is set to notify waiting clients that the call was completed.
        /// </summary>
        protected AutoResetEvent callFinished = new AutoResetEvent(false);

        /// <summary>
        /// The call states.
        /// </summary>
        protected enum State { InProgress, Success, Exception, Error };

        /// <summary>
        /// The current call state.
        /// </summary>
        protected State state = State.InProgress;

        /// <summary>
        /// The cached result to be passed into call handlers added after the call was completed. This may be either a
        /// returned value, an exception or an error reason depending on the current <see cref="state"/>. In the latter
        /// two cases the types would be an Exception and a string respectively. In the first case, type could be any
        /// and will be casted to required type using <see cref="convertResult"/>.
        /// </summary>
        protected object cachedResult = null;
    }
}

