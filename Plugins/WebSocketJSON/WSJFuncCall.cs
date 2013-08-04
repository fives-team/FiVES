using System;
using KIARA;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SuperWebSocket;
using System.Threading;
using System.Diagnostics;

namespace WebSocketJSON
{
    public class WSJFuncCall : FuncCall
    {
        #region FuncCall implementation

        public FuncCall onSuccess<T>(Action<T> handler)
        {
            if (state == State.InProgress)
                successHandlers.Add(handler);
            else if (state == State.Success)
                handler(((JToken)result).ToObject<T>());
            return this;
        }

        public FuncCall onException(Action<Exception> handler)
        {
            if (state == State.InProgress)
                exceptionHandlers.Add(handler);
            else if (state == State.Exception)
                handler((Exception)result);
            return this;
        }

        public FuncCall onError(Action<string> handler)
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
        }

        #endregion

        internal void handleSuccess(JToken retValue)
        {
            state = State.Success;
            result = retValue;

            foreach (var handler in successHandlers) {
                Debug.Assert(handler.Method.GetParameters().Length == 1);
                Type argType = handler.Method.GetParameters()[0].GetType();
                handler.DynamicInvoke(retValue.ToObject(argType));
            }

            successHandlers.Clear();
            callFinished.Set();
        }

        internal void handleException(JToken exception)
        {
            state = State.Exception;
            result = new Exception(exception.ToString());

            foreach (var handler in exceptionHandlers)
                handler((Exception)result);

            exceptionHandlers.Clear();
            callFinished.Set();
        }

        internal void handleError(JToken error)
        {
            state = State.Error;
            result = error.ToString();

            foreach (var handler in errorHandlers)
                handler((string)result);

            errorHandlers.Clear();
            callFinished.Set();
        }

        private List<Delegate> successHandlers = new List<Delegate>();
        private List<Action<Exception>> exceptionHandlers = new List<Action<Exception>>();
        private List<Action<string>> errorHandlers = new List<Action<string>>();
        private AutoResetEvent callFinished = new AutoResetEvent(false);

        private enum State { InProgress, Success, Exception, Error };
        private State state = State.InProgress;
        private object result = null;
    }
    
}
