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
            formCount = 0;
        }

        public XmlNode Render()
        {
            RenderWidgetContainer();
            RenderValues();
            RenderForms();
            RenderActions();
            return Root as XmlNode;
        }

        protected abstract void RenderValues();
        protected abstract void RenderActions();
        protected abstract void RenderForms();

        protected void RenderWidgetContainer()
        {
            Root = DiagnosisInterface.Instance.WidgetTemplate.Clone() as XmlDocument;
            Root.SelectSingleNode("//*[@name='plugin-label']").InnerText = ParentPlugin.Name;
            ValueTable = Root.SelectSingleNode("//*[@name='value-table']");
            UpdateFormList = Root.SelectSingleNode("//*[@name='update-forms']");
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

        protected void renderUpdateForm(string updateFunctionName, Dictionary<string, string> parameters)
        {
            formCount++;
            string formID = ParentPlugin.Name + "-form" + formCount.ToString();
            int attrCount = 0;

            var form = Root.CreateElement("form");
            form.SetAttribute("id", formID);
            form.SetAttribute("class", "form-horizontal");
            form.SetAttribute("action", String.Format("/diagnosis/action/{0}/{1}", ParentPlugin.Name, updateFunctionName));
            form.SetAttribute("method", "POST");

            foreach(KeyValuePair<string, string> parameter in parameters)
            {
                attrCount++;
                var formGroup = Root.CreateElement("div");
                formGroup.SetAttribute("class", "form-group");

                var label = Root.CreateElement("label");
                label.SetAttribute("for", parameter.Key);
                label.SetAttribute("class", "col-sm-3 control-label");
                label.InnerText = parameter.Key;

                var inputDiv = Root.CreateElement("div");
                inputDiv.SetAttribute("class", "col-sm-9");

                var input = Root.CreateElement("input");
                input.SetAttribute("id", formID + "-attr" + attrCount);
                input.SetAttribute("name", parameter.Key);
                input.SetAttribute("type", "text");
                input.SetAttribute("class", "form-control");
                input.SetAttribute("value", parameter.Value);

                inputDiv.AppendChild(input);

                formGroup.AppendChild(label);
                formGroup.AppendChild(inputDiv);

                form.AppendChild(formGroup);
            }

            var button = Root.CreateElement("button");
            button.SetAttribute("class", "btn btn-info");
            button.InnerText = "Update values";

            form.AppendChild(button);
            UpdateFormList.AppendChild(form);
        }

        XmlDocument Root;
        XmlNode ValueTable;
        XmlNode ActionButtonList;
        XmlNode UpdateFormList;
        int formCount;
    }
}
