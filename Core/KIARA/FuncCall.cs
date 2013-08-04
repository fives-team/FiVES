using System;

namespace KIARA
{
    public interface FuncCall
    {
        // Sets up a success |handler|, which should accept a single argument of any type and have no return value.
        // Return value from the remote call will be converted to the type of the first argument and passed to the
        // |handler|.
        FuncCall onSuccess<T>(Action<T> handler);

        // Sets up an exception |handler|, which gets passed an exception that was raised.
        FuncCall onException(Action<Exception> handler);

        // Sets up an error |handler|, which gets passed a reason for the error.
        FuncCall onError(Action<string> handler);

        // Executes the call syncrhonously. Converts a value returned from the call into type T and returns it. On
        // error a KIARA.Error exception is raised. Remote exceptions are raised locally. All assigned handlers for this
        // call are executed before returning from this call. Times out after |millisecondsTimeout| or waits 
        // indefinitely if the value is -1.
        T wait<T>(int millisecondsTimeout = -1);

        // Executes the call syncrhonously. Return value (if any) is ignored. On error a KIARA.Error exception is
        // raised. Remote exceptions are raised locally. All assigned handlers for this call are executed before
        // returning from this call. Times out after |millisecondsTimeout| or waits indefinitely if the value is -1.
        void wait(int millisecondsTimeout = -1);
    }
}

