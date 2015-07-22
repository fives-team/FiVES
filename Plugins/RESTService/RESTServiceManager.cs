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
    class RESTServiceManager
    {
        public static RESTServiceManager Instance;
        private  HttpListener _listener = new HttpListener();

        public RESTServiceManager(string prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            _listener.Prefixes.Add(prefixes);

            _listener.Start();
        }

        internal void Initialize()
        {
            this.Run();
        }

        private void Run()
        {
            ThreadPool.QueueUserWorkItem( _ =>
            {
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem( context =>
                    handleRequest(context), _listener.GetContext());
                }
            });
        }

        private void handleRequest(object ctx)
        {
            var context = ctx as HttpListenerContext;
            try
            {
                HttpListenerRequest request = context.Request;
                string httpMethod = request.HttpMethod;
                string rawUrl = request.RawUrl;

                System.IO.Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

                string content = reader.ReadToEnd();
                body.Close();
                reader.Close();

                JObject jObject = null;
                if (!content.Equals(""))
                {
                    jObject = JsonParser(content);
                }

                // TODO:
                // Here would be the point to dispatch the request to some registered handler
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

        private JObject JsonParser(string json)
        {
            JObject o = JObject.Parse(json);
            return o;
        }
    }
}
