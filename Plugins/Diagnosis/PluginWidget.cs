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
            RenderWidgetContainer();
            RenderValues();
            RenderActions();
            return Root;
        }

        protected abstract XmlNode RenderValues();
        protected abstract XmlNode RenderActions();

        protected void RenderWidgetContainer()
        {
            Root = DiagnosisInterface.Instance.WidgetTemplate.Clone() as XmlDocument;
            Root.SelectSingleNode("/div/div[@name='plugin-label']").InnerText = ParentPlugin.Name;
            ValueTable = Root.SelectSingleNode("/div/div/[@class='panel-body']/table");
            ActionButtonList = Root.SelectSingleNode("/div/div/[@class='panel-body']/div");
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
