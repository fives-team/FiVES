using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace WebTests
{
    class WebServer
    {
        public int ServerPort
        {
            get
            {
                return serverPort;
            }
            set
            {
                if (running)
                    throw new Exception("Can't change port on a running server.");
                else
                    serverPort = value;
            }
        }

        public string RootDir
        {
            get
            {
                return rootDir;
            }
            set
            {
                if (running)
                    throw new Exception("Can't change port on a running server.");
                else
                    rootDir = value;
            }
        }

        public void Start()
        {
            running = true;

            listener = new HttpListener();
            listener.Prefixes.Add(String.Format("http://localhost:{0}/", serverPort));
            listener.Start();

            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            stopEvent.Set();

            listenerThread.Join();
            listenerThread = null;

            listener.Stop();
            listener = null;

            running = false;

            stopEvent.Reset();
        }

        private void ListenerThread()
        {
            while (listener.IsListening)
            {
                var context = listener.BeginGetContext(ContextReady, null);
                if (0 == WaitHandle.WaitAny(new[] { stopEvent, context.AsyncWaitHandle }))
                    break;
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                HttpListenerContext context = listener.EndGetContext(ar);

                string filename = context.Request.Url.AbsolutePath;
                filename = filename.Substring("/".Length);
                if (string.IsNullOrEmpty(filename))
                    filename = "client.html";
                filename = Path.Combine(rootDir, filename);

                if (!File.Exists(filename))
                {
                    context.Response.StatusCode = 404;
                    context.Response.OutputStream.Close();
                }
                else
                {
                    string extension = Path.GetExtension(filename).ToLower();
                    if (extension == ".js")
                        context.Response.Headers.Add("Content-Type", "application/javascript");
                    else if (extension == ".css")
                        context.Response.Headers.Add("Content-Type", "text/css");
                    else if (extension == ".json")
                        context.Response.Headers.Add("Content-Type", "application/json");
                    else if (extension == ".xhtml")
                        context.Response.Headers.Add("Content-Type", "application/xhtml+xml");
                    else if (extension == ".xml")
                        context.Response.Headers.Add("Content-Type", "text/xml");

                    Stream input = new FileStream(filename, FileMode.Open);
                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();
                    context.Response.OutputStream.Close();
                }
            }
            catch
            {
                return;
            }
        }

        private HttpListener listener = null;
        private Thread listenerThread = null;
        private int serverPort = 44838;
        private string rootDir = Directory.GetCurrentDirectory();
        private volatile bool running = false;
        private ManualResetEvent stopEvent = new ManualResetEvent(false);
    }
}
