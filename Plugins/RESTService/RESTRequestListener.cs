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
using FIVES;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace RESTServicePlugin
{
    /// <summary>
    /// Used by the REST Plugin to open an HTTP Listener, receive incoming requests and hand them to the
    /// <see cref="RequestDispatcher">RequestDispatcher</see>
    /// </summary>
    class RESTRequestListener
    {
        private  HttpListener Listener = new HttpListener();
        private Task ListenerLoop;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private bool isListening = false;
        private string baseURL;

        public RESTRequestListener(string host, string path)
        {
            this.baseURL = path;
            string prefixes = host + path;
            Listener.Prefixes.Add(prefixes);
            Listener.Start();
        }

        internal void Initialize()
        {
            this.Run();
            isListening = true;
        }

        internal void Shutdown()
        {
            isListening = false;
        }

        private void Run()
        {
            ListenerLoop = Task.Factory.StartNew( () =>
            {
                while (isListening)
                {
                    handleRequest(Listener.GetContext());
                }

                tokenSource.Cancel();
                Listener.Close();
            }, tokenSource.Token);
        }

        private void handleRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                string content = readRequestContent(request);
                // we want to keep the first '/'
                string path = request.RawUrl.Remove(1, baseURL.Length - 1);
                var response = RequestDispatcher.Instance.DispatchRequest(path, request.HttpMethod, content);
                context.Response.StatusCode = response.ReturnCode;
                if (response.ResponseBuffer != null && response.ResponseBuffer.Length > 0)
                    context.Response.OutputStream.Write(response.ResponseBuffer, 0, response.ResponseBuffer.Length);
            }
            catch (NotImplementedException e)
            {
                context.Response.StatusCode = 501;
                context.Response.OutputStream.Flush();
                string error =
                    "The requested service is currently not implemented. The server returned code: 501\n"
                    + "'" + e.Message + "'";
                context.Response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes(error), 0, error.Length);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.OutputStream.Flush();
                string error =
                    "An internal server error occurred while processing the request. The server returned code: 500\n"
                    + "'" + e.Message + "'";
                context.Response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes(error), 0, error.Length);
            }

            finally
            {
                // always close the stream
                context.Response.OutputStream.Close();
            }
        }

        private string readRequestContent(HttpListenerRequest request)
        {
            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

            string content = reader.ReadToEnd();
            body.Close();
            reader.Close();

            return content;
        }

    }
}
