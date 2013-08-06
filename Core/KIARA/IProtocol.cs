using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace KIARA
{
    public interface IProtocol
    {
        // Processes parsed KIARA IDL file.
        // TODO: We should pass parsed IDL instead of a string, when the parsing is implemented.
        void processIDL(string parsedIDL);

        // Calls function |name| with |args| on the remote end. Returns FuncCall object which allows users to set up 
        // callbacks for various outcomes of the call or wait for the call completion (synchronous execution).
        IFuncCall callFunc(string name, params object[] args);

        // Registers a |handler| for the function |name|, which is called when remote end requests the function to be
        // called. Returned value from |handler| (if any) is returned back to the caller. If called again for the same
        // |name| - throws an exception.
        void registerHandler(string name, Delegate handler);
    }
}
