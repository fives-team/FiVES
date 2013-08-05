using System;
using FIVES;
using System.Collections.Generic;
using KIARA;

namespace DirectCall
{
    class DCProtocol : IProtocol
    {
        #region IProtocol implementation

        public void processIDL(string parsedIDL)
        {
            throw new NotImplementedException();
        }

        public IFuncCall callFunc(string name, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void registerHandler(string name, Delegate handler)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

