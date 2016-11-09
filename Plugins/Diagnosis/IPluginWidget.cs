using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DiagnosisPlugin
{
    public abstract class IPluginWidget
    {
        XmlNode Render()
        {
            Root = new XmlDocument();
            Root.AppendChild(RenderValues());
            Root.AppendChild(RenderActions());
            return Root;
        }

        public abstract XmlNode RenderValues();
        public abstract XmlNode RenderActions();

        protected void renderWidgetContainer()
        {

        }

        protected void renderTableRow()
        {

        }

        protected void renderActionButton()
        {

        }

        XmlDocument Root;
        HashSet<PluginValue> Values;
        HashSet<PluginAction> Actions;
    }
}
