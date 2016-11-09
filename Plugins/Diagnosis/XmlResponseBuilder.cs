using FIVES;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace DiagnosisPlugin
{
    class XmlResponseBuilder
    {
        private XmlDocument ResponseDocument;

        public XmlResponseBuilder()
        {
        }

        public XmlDocument RenderResponse()
        {
            ParseTemplateAsXml();
            RenderBaseInfo();
            RenderPluginWidgets();
            return ResponseDocument;
        }

        private void RenderBaseInfo()
        {
            // TODO: Base info direkt in XML injecten
            string loadedPlugins = getLoadedPlugins();
            string deferredPlugins = getDeferredPlugins();

            ResponseDocument.SelectSingleNode("//*[@id='entities-body']").InnerText = World.Instance.Count.ToString();
            ResponseDocument.SelectSingleNode("//*[@id='loaded-plugins-area']").InnerText = loadedPlugins;
            ResponseDocument.SelectSingleNode("//*[@id='deferred-plugins-area']").InnerText = deferredPlugins;
        }

        private string getLoadedPlugins()
        {
            string loadedPlugins = "";
            foreach (PluginManager.PluginInfo pi in PluginManager.Instance.LoadedPlugins)
            {
                loadedPlugins += pi.Name + '\n';
            }
            return loadedPlugins;
        }

        private string getDeferredPlugins()
        {
            string deferredPlugins = "";
            foreach (PluginManager.PluginInfo pi in PluginManager.Instance.DeferredPlugins)
            {
                deferredPlugins += pi.Name + '\n';
                if (pi.RemainingComponentDeps.Count > 0)
                {
                    deferredPlugins += '\t' + "missing components:" + '\n';
                    foreach (string component in pi.RemainingComponentDeps)
                    {
                        deferredPlugins += "\t\t" + component + '\n';
                    }
                }
                if (pi.RemainingPluginDeps.Count > 0)
                {
                    deferredPlugins += '\t' + "missing plugins:" + '\n';
                    foreach (string plugin in pi.RemainingPluginDeps)
                    {
                        deferredPlugins += "\t\t" + plugin + '\n';
                    }
                }
            }
            return deferredPlugins;
        }

        private void RenderPluginWidgets()
        {
            foreach (PluginContext c in DiagnosisInterface.Instance.Contexts)
            {
                //Render (c.GetPluginWidget());
                ResponseDocument.SelectSingleNode(pluginWidgetPath).AppendChild(c.GetPluginWidget().Render());
            }
        }

        void ParseTemplateAsXml()
        {
            ResponseDocument = new XmlDocument();
            ResponseDocument.Load("DiagnosisWebpage/dynamic/index.html");
        }
    }
}
