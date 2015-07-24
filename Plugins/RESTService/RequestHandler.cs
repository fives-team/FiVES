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
using System.Net;
using System.Text;

namespace RESTServicePlugin
{
    public abstract class RequestHandler
    {
        private string currentRequestContent;

        public abstract string Path { get; }
        public abstract string ContentType { get; }
        protected abstract RequestResponse HandleGET();
        protected abstract RequestResponse HandlePOST();
        protected abstract RequestResponse HandlePUT();
        protected abstract RequestResponse HandleDELETE();

        public RequestResponse HandleRequest(string httpMethod, string path, string content)
        {
            currentRequestContent = content;

            switch(httpMethod)
            {
                case "GET": return HandleGET();
                case "POST": return HandlePOST();
                case "PUT": return HandlePUT();
                case "DELETE": return HandleDELETE();
                default: return ConstructServerError();
            }
        }

        protected RequestResponse ConstructServerError()
        {
            RequestResponse errorResponse = new RequestResponse();
            errorResponse.ReturnCode = 500;
            errorResponse.SetResponseBuffer("Some error occured when trying to process request for " + Path);
            return errorResponse;
        }
    }
}
