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
            stopRequested = true;
        }

        private void ThreadFunc()
        {
            while (!stopRequested)
            {
                HttpListenerContext context = listener.GetContext();
                string filename = context.Request.Url.AbsolutePath;
                filename = filename.Substring(1);
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
                    Stream input = new FileStream(filename, FileMode.Open);
                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();
                    context.Response.OutputStream.Close();
                }
            }

            listener.Stop();
            stopRequested = false;
            running = false;
            listener = null;
            processingThread = null;
        }

        private HttpListener listener = null;
        private Thread processingThread = null;
        private int serverPort = 44838;
        private bool running = false;
        private string rootDir = Directory.GetCurrentDirectory();
        private volatile bool stopRequested = false;
    }
}
