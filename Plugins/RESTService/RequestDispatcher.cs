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
    public class RequestDispatcher
    {
        private static RequestDispatcher instance = new RequestDispatcher();
        public static RequestDispatcher Instance { get { return instance;} }

        public RequestResponse DispatchRequest(string path, string httpmethod, string content)
        {
            RequestHandler handler = registeredHandlers.Find(h => path.StartsWith(h.Path));

            if (handler == null)
                return SendErrorResponse();

            return handler.HandleRequest(httpmethod, content);
        }

        public void RegisterHandler(RequestHandler newHandler)
        {
            RequestHandler handler = registeredHandlers.Find(h => h.Path == newHandler.Path);
            if (handler != null)
                throw new ArgumentException("A request handler for the path " + newHandler.Path +
                    " is already registered");

            registeredHandlers.Add(newHandler);
        }

        private RequestResponse SendErrorResponse()
        {
            RequestResponse errorResponse = new RequestResponse();
            errorResponse.ReturnCode = 404;
            errorResponse.SetResponseBuffer("No service is registered under the specified path");
            return errorResponse;
        }

        private List<RequestHandler> registeredHandlers = new List<RequestHandler>();
    }
}
