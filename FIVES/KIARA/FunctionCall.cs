using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace KIARA
{
    // Availiable handler types for events.
    // Result handler. Result can be either an |exception| or a |returnValue| from the function. Only one of them
    // will be non-null. You may also specify your own delegate type - it must have two parameters and return void.
    // KIARA will try to convert exception and returnValue to desired types automatically.
    public delegate void CallResultCallback(JObject exception, JObject returnValue);

    // Error handler. It is executed when a connection error has happened and function execution status is unknown.
    public delegate void CallErrorCallback(string reason);

    public class FunctionCall
    {
        #region Public interface
        // Registers a |handler| for the event with |eventName|. Supported event names: error, result. See corresponding 
        // handler types above.
        public FunctionCall On(string eventName, Delegate handler)
        {
            ValidateHandler(eventName, handler);

            if (eventName == "result")
                OnResult.Add(handler);
            else if (eventName == "exception")
                OnException.Add(handler);
            else if (eventName == "error")
                OnError += (CallErrorCallback)handler;

            if (haveCachedResult)
                SetResult(cachedResultType, cachedArgument);

            return this;
        }
        // Allows to set up a global exception handler to handle all otherwise unhandled exceptions.
        // This will override any previously configured global exception handler.
        public static void SetGlobalExceptionHandler(Delegate exceptionHandler)
        {
            globalExceptionHandler = exceptionHandler;
        }
        #endregion

        #region Private implementation
        // This will be called after we loose all references to the call object. This means that no
        // new handlers may be added. Then if we still have an exception as result and it haven't
        // been handled, we either invoke a global exception handler if one was set or raise this
        // exception.
        ~FunctionCall()
        {
            if (haveCachedResult && cachedResultType == "exception" && !exceptionHaveBeenHandled) {
                if (globalExceptionHandler != null) {
                    Type retValueType =
                        globalExceptionHandler.Method.GetParameters()[0].ParameterType;
                    globalExceptionHandler.DynamicInvoke(
                        ConversionUtils.CastJObject(cachedArgument, retValueType));
                } else {
                    throw new Error(ErrorCode.GENERIC_ERROR, "Received unhandled exception from " +
                        "the remote end: " + cachedArgument.ToString()
                    );
                }
            }
        }

        internal void SetResult(string resultType, object argument)
        {
            if (resultType == "result") {
                // Cast return object to a specific type accepted by each individual result handler.
                foreach (Delegate resultDelegate in OnResult) {
                    Type retValueType = resultDelegate.Method.GetParameters()[0].ParameterType;
                    resultDelegate.DynamicInvoke(
                        ConversionUtils.CastJObject(argument, retValueType));
                }

                foreach (Delegate excResultDelegate in OnExcResult) {
                    Type retValueType = excResultDelegate.Method.GetParameters()[1].ParameterType;
                    excResultDelegate.DynamicInvoke(
                        null, ConversionUtils.CastJObject(argument, retValueType));
                }
            } else if (resultType == "exception") {
                // Cast exception object to a specific type accepted by each individual result 
                // handler.
                foreach (Delegate exceptionDelegate in OnException) {
                    Type retValueType = exceptionDelegate.Method.GetParameters()[0].ParameterType;
                    exceptionDelegate.DynamicInvoke(
                        ConversionUtils.CastJObject(argument, retValueType));
                }

                foreach (Delegate excResultDelegate in OnExcResult) {
                    Type exceptionType = excResultDelegate.Method.GetParameters()[0].ParameterType;
                    excResultDelegate.DynamicInvoke(
                        ConversionUtils.CastJObject(argument, exceptionType), null);
                }

                // If no handlers are set, yet exception was returned - raise it.
                if (OnException.Count > 0 || OnExcResult.Count > 0)
                    exceptionHaveBeenHandled = true;
            } else if (resultType == "error") {
                if (argument.GetType() != typeof(string)) {
                    throw new Error(ErrorCode.INVALID_ARGUMENT,
                                    "Argument for 'error' event must be a string");
                }
                if (OnError != null)
                    OnError((string)argument);
            } else
                throw new Error(ErrorCode.INVALID_ARGUMENT, "Invalid event name: " + resultType);

            // Cache current result.
            haveCachedResult = true;
            cachedResultType = resultType;
            cachedArgument = argument;

            // Clean up existing handlers so that they are not invoked again (SetResult may be
            // called again for newly added handlers).
            OnResult.Clear();
            OnException.Clear();
            OnExcResult.Clear();
            OnError = null;
        }

        private static bool ValidateNArgumentHandler(Delegate handler, int numArgs)
        {
            return handler is CallResultCallback ||
                (handler.Method.GetParameters().Length == numArgs && 
                handler.Method.ReturnType == typeof(void));
        }

        internal static void ValidateHandler(string eventName, Delegate handler)
        {
            bool valid = false;
            if (eventName == "result") 
                valid = ValidateNArgumentHandler(handler, 1);
            else if (eventName == "error")
                valid = handler is CallErrorCallback;
            else if (eventName == "exception")
                valid = ValidateNArgumentHandler(handler, 1);
            else if (eventName == "exc_result")
                valid = ValidateNArgumentHandler(handler, 2);
            else
                throw new Error(ErrorCode.INVALID_ARGUMENT, "Unknown event name: " + eventName);
            if (!valid) {
                throw new Error(ErrorCode.INVALID_ARGUMENT,
                                "Unsupported handler type " + handler.GetType().Name + " for " + eventName
                );
            }
        }

        internal List<Delegate> OnResult = new List<Delegate>();
        internal List<Delegate> OnException = new List<Delegate>();
        internal List<Delegate> OnExcResult = new List<Delegate>();

        internal event CallErrorCallback OnError;

        private string cachedResultType = null;
        private object cachedArgument = null;
        private bool haveCachedResult = false;
        private bool exceptionHaveBeenHandled = false;
        private static Delegate globalExceptionHandler = null;
        #endregion
    }
}
