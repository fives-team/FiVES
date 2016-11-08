using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DiagnosisPlugin
{
    abstract class IPluginWidget
    {
        XmlNode Render()
        {
            Root = new XmlDocument();
            Root.AppendChild(RenderValues());
            Root.AppendChild(RenderActions());
            return Root;
        }

        virtual XmlNode RenderValues();
        virtual XmlNode RenderActions();

        XmlDocument Root;
        HashSet<PluginValue> Values;
        HashSet<PluginAction> Actions;
    }
}
