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
    /// <summary>
    /// The RequestDispatcher can be used to register new REST Services. For this, implement a class that is derived
    /// from the <see cref="RequestHandler">RequestHandler</see> class. Each incoming request is handed to a respective
    /// handler implementation that is registered for the request's base URL
    /// </summary>
    public class RequestDispatcher
    {
        private static RequestDispatcher instance = new RequestDispatcher();

        /// <summary>
        /// Singleton Instance of the Request dispatcher that can be used by other plugins to register own
        /// REST services
        /// </summary>
        public static RequestDispatcher Instance { get { return instance;} }

        /// <summary>
        /// Checks the path of the incoming request and hands to the respective request handler
        /// </summary>
        /// <param name="path">Path that was requested</param>
        /// <param name="httpmethod">The HTTP method of the request</param>
        /// <param name="content">Content that was submitted with the request (the body)</param>
        /// <returns></returns>
        public RequestResponse DispatchRequest(string path, string httpmethod, string content)
        {
            RequestHandler handler = registeredHandlers.Find(h => path.StartsWith(h.Path));

            if (handler == null)
                return SendErrorResponse();

            return handler.HandleRequest(httpmethod, path, content);
        }

        /// <summary>
        /// Registers a new <seealso cref="RequestHandler">RequestHandler</seealso> implementation
        /// </summary>
        /// <param name="newHandler">The request handler to register.</param>
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
