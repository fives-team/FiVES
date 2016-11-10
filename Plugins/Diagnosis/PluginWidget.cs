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
            XmlNode pluginLabel = Root.SelectSingleNode("//*[@name='plugin-label']");

            XmlElement panelBody = Root.SelectSingleNode("//*[@class='panel-body']") as XmlElement;
            panelBody.SetAttribute("id", ParentPlugin.Name + "-body");
            panelBody.SetAttribute("class", "panel-body collapse in");

            XmlElement collapseLink = Root.CreateElement("a");
            collapseLink.SetAttribute("role", "button");
            collapseLink.SetAttribute("data-toggle", "collapse");
            collapseLink.SetAttribute("aria-expanded", "true");
            collapseLink.SetAttribute("href", "#" + ParentPlugin.Name + "-body");
            collapseLink.InnerText = ParentPlugin.Name;

            pluginLabel.AppendChild(collapseLink);

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

        protected void renderActionButton(string name)
        {
            renderActionButton(name, new Dictionary<string, string>());
        }

        protected void renderActionButton(string name, Dictionary<string, string> parameters)
        {
            var buttonElement = Root.CreateElement("button");
            buttonElement.SetAttribute("class", "btn btn-info");
            buttonElement.SetAttribute("onclick", String.Format("callMethod('{0}','{1}')", ParentPlugin.Name, name));
            buttonElement.InnerText = name;
            ActionButtonList.AppendChild(buttonElement);
            if (parameters.Count > 0)
                renderUpdateForm(name, parameters);
        }

        protected void renderUpdateForm(string methodName, Dictionary<string, string> parameters)
        {
            string formID = String.Format("form-{0}-{1}", ParentPlugin.Name, methodName);
            var form = Root.CreateElement("form");

            form.SetAttribute("id", formID);
            form.SetAttribute("class", "form-horizontal");

            foreach(KeyValuePair<string, string> parameter in parameters)
            {
                var formGroup = Root.CreateElement("div");
                formGroup.SetAttribute("class", "form-group");

                var label = createParamLabel(parameter.Key);
                var inputDiv = Root.CreateElement("div");
                inputDiv.SetAttribute("class", "col-sm-9");
                var input = createParamInput(methodName, parameter);

                inputDiv.AppendChild(input);
                formGroup.AppendChild(label);
                formGroup.AppendChild(inputDiv);

                form.AppendChild(formGroup);
            }

            ActionButtonList.AppendChild(form);
        }

        private XmlNode createParamLabel(string paramName)
        {
            var label = Root.CreateElement("label");
            label.SetAttribute("for", paramName);
            label.SetAttribute("class", "col-sm-3 control-label");
            label.InnerText = paramName;
            return label;
        }

        private XmlNode createParamInput(string methodName, KeyValuePair<string, string> parameterDefinition)
        {
            var input = Root.CreateElement("input");
            input.SetAttribute("id",
                String.Format("param-{0}-{1}-{2}", ParentPlugin.Name, methodName, parameterDefinition.Key));
            input.SetAttribute("name", parameterDefinition.Key);
            input.SetAttribute("type", "text");
            input.SetAttribute("class", "form-control");
            input.SetAttribute("value", parameterDefinition.Value);
            return input;
        }

        XmlDocument Root;
        XmlNode ValueTable;
        XmlNode ActionButtonList;
    }
}
