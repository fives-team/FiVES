﻿// This file is part of FiVES.
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
    class RESTRequestListener
    {
        private  HttpListener Listener = new HttpListener();
        private Task ListenerLoop;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private bool isListening = false;
        private string baseURL;

        public RESTRequestListener(string prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            baseURL = prefixes;
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
                string path = getRequestPath(request.Url);
                var response = RequestDispatcher.Instance.DispatchRequest(path, request.HttpMethod, content);
                context.Response.StatusCode = response.ReturnCode;
                context.Response.OutputStream.Write(response.ResponseBuffer, 0, response.ResponseBuffer.Length);
            }
            catch
            {
                Console.WriteLine("not supported");
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

        private string getRequestPath(Uri fullPath)
        {
            return fullPath.ToString().Replace(baseURL, "/");
        }

        private JObject JsonParser(string json)
        {
            JObject o = JObject.Parse(json);
            return o;
        }

    }
}