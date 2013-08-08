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

    /// <summary>
    /// Represents an error raised by KIARA.
    /// </summary>
    public class Error : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KIARA.Error"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="reason">The reason.</param>
        public Error(ErrorCode code, string reason)
        {
            Code = code;
            Reason = reason;
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>The error code.</value>
        public ErrorCode Code { get; private set; }

        /// <summary>
        /// Gets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; private set; }
    }
}
