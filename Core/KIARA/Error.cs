using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KIARA
{
    public enum ErrorCode
    {
        SUCCESS = 0,
        NO_ERROR = SUCCESS,
        GENERIC_ERROR = 0x0001,
        INPUT_ERROR = 0x0100,
        OUTPUT_ERROR = 0x0200,
        CONNECTION_ERROR = 0x0300,
        IDL_LOAD_ERROR = 0x0301,
        API_ERROR = 0x0500,
        INIT_ERROR = 0x0501,
        FINI_ERROR = 0x0502,
        INVALID_VALUE = 0x0503,
        INVALID_TYPE = 0x0504,
        INVALID_OPERATION = 0x0505,
        INVALID_ARGUMENT = 0x0506,
        UNSUPPORTED_FEATURE = 0x0507
    }

    public class Error : Exception
    {
        public Error(ErrorCode code, string reason)
        {
            Code = code;
            Reason = reason;
        }

        public ErrorCode Code { get; private set; }

        public string Reason { get; private set; }
    }
}
