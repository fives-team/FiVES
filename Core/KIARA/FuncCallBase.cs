using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace KIARA
{
    public abstract class FuncCallBase : IFuncCall
    {
        #region IFuncCall implementation

        public IFuncCall onSuccess<T>(Action<T> handler)
        {
            if (state == State.InProgress)
                successHandlers.Add(handler);
            else if (state == State.Success)
                handler((T)convertResult(result, typeof(T)));
            return this;
        }

        public IFuncCall onSuccess(Action handler)
        {
            if (state == State.InProgress)
                successHandlers.Add(handler);
            else if (state == State.Success)
                handler();
            return this;
        }

        public IFuncCall onException(Action<Exception> handler)
        {
            if (state == State.InProgress)
                exceptionHandlers.Add(handler);
            else if (state == State.Exception)
                handler((Exception)result);
            return this;
        }

        public IFuncCall onError(Action<string> handler)
        {
            if (state == State.InProgress)
                errorHandlers.Add(handler);
            else if (state == State.Error)
                handler((string)result);
            return this;
        }

        public T wait<T>(int millisecondsTimeout = -1)
        {
            T result = default(T);
            onSuccess<T>(delegate (T value) { result = value; });
            wait(millisecondsTimeout);
            return result;
        }

        public void wait(int millisecondsTimeout = -1)
        {
            if (millisecondsTimeout == -1)
                callFinished.WaitOne();
            else
                callFinished.WaitOne(millisecondsTimeout);

            if (state == State.Error)
                throw new Error(ErrorCode.CONNECTION_ERROR, "Error during the call. Reason: " + (string)result);
            else if (state == State.Exception)
                throw (Exception)result;
            else if (state == State.InProgress)
                throw new TimeoutException("Call timed out after " + millisecondsTimeout + "ms");
        }

        #endregion

        public virtual void handleSuccess(object retValue)
        {
            state = State.Success;
            result = retValue;

            foreach (var handler in successHandlers) {
                Debug.Assert(handler.Method.GetParameters().Length <= 1);
                if (handler.Method.GetParameters().Length == 0) {
                    handler.DynamicInvoke();
                } else {
                    Type argType = handler.Method.GetParameters()[0].GetType();
                    handler.DynamicInvoke(convertResult(retValue, argType));
                }
            }

            successHandlers.Clear();
            callFinished.Set();
        }

        public virtual void handleException(Exception exception)
        {
            state = State.Exception;
            result = exception;

            foreach (var handler in exceptionHandlers)
                handler(exception);

            exceptionHandlers.Clear();
            callFinished.Set();
        }

        public virtual void handleError(string reason)
        {
            state = State.Error;
            result = reason;

            foreach (var handler in errorHandlers)
                handler(reason);

            errorHandlers.Clear();
            callFinished.Set();
        }

        // Subclasses need to override this method to convert result to the type by the user callback.
        protected abstract object convertResult(object result, Type type);

        protected List<Delegate> successHandlers = new List<Delegate>();
        protected List<Action<Exception>> exceptionHandlers = new List<Action<Exception>>();
        protected List<Action<string>> errorHandlers = new List<Action<string>>();
        protected AutoResetEvent callFinished = new AutoResetEvent(false);

        protected enum State { InProgress, Success, Exception, Error };
        protected State state = State.InProgress;
        protected object result = null;
    }
}

