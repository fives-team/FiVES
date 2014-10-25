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

            processingThread = new Thread(ThreadFunc);
            processingThread.Start();
        }

        public void Stop()
        {
            listener.Stop();
            while (running)
                Thread.Sleep(50);
        }

        private void ThreadFunc()
        {
            while (true)
            {
                HttpListenerContext context;
                try
                {
                    context = listener.GetContext();
                }
                catch (HttpListenerException e)
                {
                    break;
                }
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

                    Stream input = new FileStream(filename, FileMode.Open);
                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();
                    context.Response.OutputStream.Close();
                }
            }

            listener = null;
            processingThread = null;
            running = false;
        }

        private HttpListener listener = null;
        private Thread processingThread = null;
        private int serverPort = 44838;
        private string rootDir = Directory.GetCurrentDirectory();
        private volatile bool running = false;
    }
}
