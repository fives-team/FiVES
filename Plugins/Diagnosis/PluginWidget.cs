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
            RenderWidgetContainer();
            RenderValues();
            RenderActions();
            return Root as XmlNode;
        }

        protected abstract void RenderValues();
        protected abstract void RenderActions();

        protected void RenderWidgetContainer()
        {
            Root = DiagnosisInterface.Instance.WidgetTemplate.Clone() as XmlDocument;
            Root.SelectSingleNode("//*[@name='plugin-label']").InnerText = ParentPlugin.Name;
            ValueTable = Root.SelectSingleNode("//*[@name='value-table']");
            ActionButtonList = Root.SelectSingleNode("//*[@name='action-button-list']");
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

        protected void renderActionButton(string name, int numParams)
        {
            var buttonElement = Root.CreateElement("button");
            buttonElement.SetAttribute("class", "btn btn-info");
            buttonElement.SetAttribute("onclick", String.Format("callAction('{0}','{1}')", ParentPlugin.Name, name));
            buttonElement.InnerText = name;
            ActionButtonList.AppendChild(buttonElement);
        }

        XmlDocument Root;
        XmlNode ValueTable;
        XmlNode ActionButtonList;
    }
}
