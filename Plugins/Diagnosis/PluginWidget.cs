using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DiagnosisPlugin
{
    public abstract class PluginWidget
    {
        private IDiagnosablePlugin ParentPlugin { get; set; }

        public PluginWidget(IDiagnosablePlugin plugin)
        {
            this.ParentPlugin = plugin;
        }

        public XmlNode Render()
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

        protected void renderTableRow(string label, object value)
        {
            var tableRow = Root.CreateElement("tr");
            var labelColumn = Root.CreateElement("td");
            labelColumn.InnerText = label;
            var valueColumn = Root.CreateElement("td");
            valueColumn.InnerText = value.ToString();
            tableRow.AppendChild(labelColumn);
            tableRow.AppendChild(valueColumn);
            ValueTable.AppendChild(tableRow);
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
