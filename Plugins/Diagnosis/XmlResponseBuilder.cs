using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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
        }

        private void RenderPluginWidgets()
        {
            foreach (PluginContext c in DiagnosisInterface.Instance.Contexts)
            {
                //Render (c.GetPluginWidget());
            }
        }

        void ParseTemplateAsXml()
        {
            ResponseDocument = new XmlDocument();
            ResponseDocument.Load("DiagnosisWebpage/dynamic/index.html");
        }
    }
}
