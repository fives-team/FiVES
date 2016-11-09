using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DiagnosisPlugin
{
    public abstract class PluginWidget
    {
        XmlNode Render()
        {
            Root = new XmlDocument();
            Root.AppendChild(RenderValues());
            Root.AppendChild(RenderActions());
            return Root;
        }

        protected abstract XmlNode RenderValues();
        protected abstract XmlNode RenderActions();

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
        XmlNode ValueTable;
        XmlNode ActionButtonList;
        HashSet<PluginValue> Values;
        HashSet<PluginAction> Actions;
    }
}
