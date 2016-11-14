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
            MethodList = Root.SelectSingleNode("//*[@name='action-button-list']");
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
            MethodList.AppendChild(Root.CreateElement("hr"));
            MethodList.AppendChild(renderButtonElement(name));
            if (parameters.Count > 0)
                MethodList.AppendChild(createParameterInputForm(name, parameters));
            MethodList.AppendChild(createResponseField(name, true));
            MethodList.AppendChild(createResponseField(name, false));
        }

        private XmlNode renderButtonElement(string name)
        {
            var buttonElement = Root.CreateElement("button");
            buttonElement.SetAttribute("class", "btn btn-info");
            buttonElement.SetAttribute("onclick", String.Format("callMethod('{0}','{1}')", ParentPlugin.Name, name));
            buttonElement.InnerText = name.Replace('_', ' ');
            return buttonElement;
        }

        private XmlNode createParameterInputForm(string methodName, Dictionary<string, string> parameters)
        {
            string formID = String.Format("form-{0}-{1}", ParentPlugin.Name, methodName);
            var form = Root.CreateElement("form");

            form.SetAttribute("id", formID);
            form.SetAttribute("class", "form-horizontal");

            foreach(KeyValuePair<string, string> parameter in parameters)
            {
                form.AppendChild(createParameterFormGroup(methodName, parameter));
            }
            return form;
        }

        private XmlNode createParameterFormGroup(string methodName, KeyValuePair<string, string> parameter)
        {
            var formGroup = Root.CreateElement("div");
            formGroup.SetAttribute("class", "form-group");
            formGroup.AppendChild(createParamLabel(parameter.Key));
            formGroup.AppendChild(createInputDiv(methodName, parameter));
            return formGroup;
        }

        private XmlNode createInputDiv(string methodName, KeyValuePair<string, string> parameter)
        {
            var inputDiv = Root.CreateElement("div");
            inputDiv.SetAttribute("class", "col-sm-9");
            inputDiv.AppendChild(createParamInput(methodName, parameter));
            return inputDiv;
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

        private XmlNode createResponseField(string methodName, bool success)
        {
            var responseField = Root.CreateElement("div");
            responseField.SetAttribute("class", "bg alert " + (success ? "alert-success" : "alert-danger"));
            responseField.SetAttribute("style", "display: none");
            responseField.SetAttribute("id", String.Format("{0}-response-{1}-{2}",
                (success ? "success" : "error"),
                ParentPlugin.Name,
                methodName));
            if (success)
                responseField.InnerText = "Success";
            else
                responseField.InnerText = "Error";
            return responseField;
        }
        XmlDocument Root;
        XmlNode ValueTable;
        XmlNode MethodList;
    }
}
