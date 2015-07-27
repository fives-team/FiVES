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
    /// Abstract base class that can be used to implement own REST Services. Specify the base path under
    /// which the service should be accessible. It can then be requested via
    /// [$RESTServicePlugin Base URL]/[$REQUEST_PATH]/[optional fields]
    /// </summary>
    public abstract class RequestHandler
    {
        private string currentRequestContent;

        /// <summary>
        /// Base path of the new service. The new service will then be accessible via the URL
        /// [$RESTServicePlugin Base URL]/[$REQUEST_PATH]/[optional fields]
        /// </summary>
        public abstract string Path { get; }

        /// <summary>
        /// Content type of the response that is usually returned by the service. Usually "text/xml" or
        /// "application/json"
        /// </summary>
        public abstract string ContentType { get; }
        protected abstract RequestResponse HandleGET(string requestPath);
        protected abstract RequestResponse HandlePOST(string requestPath, string content);
        protected abstract RequestResponse HandlePUT(string requestPath, string content);
        protected abstract RequestResponse HandleDELETE(string requestPath);

        /// <summary>
        /// Called by the RequestDispatcher when a request for the specific REST service was received.
        /// Delegates the request to the GET, POST, PUT or DELETE handler, depending on the http type
        /// of the received request
        /// </summary>
        /// <param name="httpMethod">The HTTP method that was used for the request</param>
        /// <param name="path">The path within the service that was requested</param>
        /// <param name="content">Content that was received together with the request</param>
        /// <returns>A request response object with the return code of the outcome of the operation and
        /// a response message (optional)</returns>
        public RequestResponse HandleRequest(string httpMethod, string path, string content)
        {
            currentRequestContent = content;
            string truncatedPath = path.Remove(0, path.IndexOf(this.Path) + this.Path.Length);
            RequestResponse response;

            switch(httpMethod)
            {
                case "GET": response = HandleGET(truncatedPath); break;
                case "POST": response = HandlePOST(truncatedPath, content); break;
                case "PUT": response = HandlePUT(truncatedPath, content); break;
                case "DELETE": response = HandleDELETE(truncatedPath); break;
                default: return ConstructServerError();
            }

            response.ContentType = this.ContentType;
            return response;
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
