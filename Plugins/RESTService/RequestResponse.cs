// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RESTServicePlugin
{
    /// <summary>
    /// Response object that contains all data about the outcome of a requested operation
    /// </summary>
    public class RequestResponse
    {
        /// <summary>
        /// HTTP Return code of the operation
        /// </summary>
        public int ReturnCode { get; set; }

        /// <summary>
        /// Content Type of the response message. By default content type of the <see cref="RequestHandler" />
        /// that sent the response
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Buffer that contains the response message
        /// </summary>
        public byte[] ResponseBuffer { get; set; }

        /// <summary>
        /// Convenience method to set the response buffer directly from a string typed message
        /// </summary>
        /// <param name="responseText">Message that should be included as response text</param>
        public void SetResponseBuffer(string responseText)
        {
            byte[] textAsByte = System.Text.Encoding.UTF8.GetBytes(responseText);
            ResponseBuffer = textAsByte;
        }
    }
}
