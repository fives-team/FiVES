// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace KIARAPlugin
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
        /// Initializes a new instance of the <see cref="KIARAPluginInitializer.Error"/> class.
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
