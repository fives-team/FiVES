using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RESTServicePlugin;
using System.IO;
using FIVES;
using ClientManagerPlugin;

namespace DiagnosisPlugin
{
    class DiagnosisRequestHandler : RequestHandler
    {
        public DiagnosisRequestHandler()
        {
        }

        public override string ContentType
        {
            get
            {
                return "text/html";
            }
        }

        public override string Path
        {
            get
            {
                return "/diagnosis";
            }
        }

        protected override RequestResponse HandleDELETE(string requestPath)
        {
            throw new NotImplementedException();
        }

        protected override RequestResponse HandleGET(string requestPath)
        {
            string sanitizedRequestPath = sanitizeRequestPath(requestPath);
            RequestResponse reqResponse = createResponse(sanitizedRequestPath);

            return reqResponse;
        }

        private string sanitizeRequestPath(string requestPath)
        {
            // register routes here. map return value to file which shall be returned
            string sanitizedPath = null;
            // TODO: redirects. some of the index paths wont work properly
            if(requestPath == "" || requestPath == "/" || requestPath == "/index" || requestPath == "/index/")
            {
                sanitizedPath = "DiagnosisWebpage/dynamic/index.html";
            }
            else if (requestPath.StartsWith("/DiagnosisWebpage/"))
            {
                sanitizedPath = requestPath.Substring(1);
                return sanitizedPath;
            }
            return sanitizedPath;
        }

        private RequestResponse createResponse(string requestPath)
        {
            // All accessible Files have to be in the DiagnosisWebpage folder
            RequestResponse reqResponse = new RequestResponse();

            reqResponse.ContentType = this.ContentType;
            string response;
            if (File.Exists(requestPath))
            {
                reqResponse.ContentType = getMimeType(requestPath);
                response = File.ReadAllText(requestPath);
                reqResponse.ReturnCode = 200;
            }
            else
            {
                response = "no such route";
                reqResponse.ReturnCode = 404;
            }

            if (requestPath.Contains("/dynamic/"))
                response = injectValues(response);

            reqResponse.SetResponseBuffer(response);
            return reqResponse;
        }

        private string getMimeType(string requestPath)
        {
            if (requestPath.EndsWith(".html")){
                return "text/html";
            }
            else if (requestPath.EndsWith(".css"))
            {
                return "text/css";
            }
            else if (requestPath.EndsWith(".js"))
            {
                return "application/javascript";
            }
            else if (requestPath.EndsWith(".ttf"))
            {
                return "application/octet-stream";
            }
            else if (requestPath.EndsWith(".woff"))
            {
                return "application/x-font-woff";
            }
            else if (requestPath.EndsWith(".woff2"))
            {
                return "application/font-woff2";
            }
            else
            {
                return null;
            }
        }

        private string injectValues(string text)
        {
            text = text.Replace("[[numOfEntities]]", World.Instance.Count.ToString());

            // we dont know if these plugins are loaded yet.
            string clientManagerStyle = "display: none;";

            string loadedPlugins = "";
            // read all loaded plugins and check for specific plugins whether if they are loaded
            foreach (PluginManager.PluginInfo pi in PluginManager.Instance.LoadedPlugins)
            {
                loadedPlugins += pi.Name + '\n';
                if(pi.Name == "ClientManager")
                {
                    clientManagerStyle = "visibility: show;";
                }
            }

            // apply visibility for specific plugin fields
            text = text.Replace("[[clientManagerStyle]]", clientManagerStyle);

            text = text.Replace("[[loadedPlugins]]", loadedPlugins);

            string deferredPlugins = "";
            foreach (PluginManager.PluginInfo pi in PluginManager.Instance.DeferredPlugins)
            {
                deferredPlugins += pi.Name + '\n';
                if (pi.RemainingComponentDeps.Count > 0)
                {
                    deferredPlugins += "\tmissing components:\n";
                    foreach (string component in pi.RemainingComponentDeps)
                    {
                        deferredPlugins += "\t\t" + component + '\n';
                    }
                }
                if (pi.RemainingPluginDeps.Count > 0)
                {
                    deferredPlugins += "\tmissing plugins:\n";
                    foreach (string plugin in pi.RemainingPluginDeps)
                    {
                        deferredPlugins += "\t\t" + plugin + '\n';
                    }
                }
            }

            text = text.Replace("[[deferredPlugins]]", deferredPlugins);

            foreach (PluginContext c in DiagnosisInterface.Instance.Contexts)
            {
                //Render (c.GetPluginWidget());
            }

            return text;
        }

        protected override RequestResponse HandlePOST(string requestPath, string content)
        {
            throw new NotImplementedException();
        }

        protected override RequestResponse HandlePUT(string requestPath, string content)
        {
            throw new NotImplementedException();
        }

        private string pluginString = "<div class=\"panel panel-primary\" id=\"content\">" +
            "\n\t<div class=\"panel-heading\">[[pluginName]]</div>" +
            "\n\t<div class=\"panel-body\">" +
            "\n\t\t[[pluginValues]]" +
            "\n\t\t[[rpcButtons]]" +
            "\n\t</div>" +
            "\n</div>" +
            "\n[[additionalPlugins]]";

        private string valueString = "<div class=\"panel-group col-md-6 col-lg-3\">" +
            "\n\t<div class=\"panel panel-info\">" +
            "\n\t\t<div class=\"panel-heading\">[[registeredKey]]</div>" +
            "\n\t\t<div class=\"panel-body\">" +
            "\n\t\t\t[[registeredValue]]" +
            "\n\t\t</div>" +
            "\n\t</div>" +
            "\n</div>" +
            "\n[[pluginValues]]";

        private string rpcString = "[[rpcButtons]]";
    }
}
