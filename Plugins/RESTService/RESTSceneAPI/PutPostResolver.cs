// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;

namespace RESTServicePlugin
{
    public class PutPostResolver
    {
        private XmlDocument ResponseDocument;
        private Entity UpdatedEntity;

        public PutPostResolver(XmlDocument responseDocumet)
        {
            this.ResponseDocument = responseDocumet;
        }

        public XmlElement AddEntity(string requestPath, string requestBody)
        {
            XmlDocument receivedDocument = new XmlDocument();
            receivedDocument.LoadXml(requestBody);

            if (receivedDocument.FirstChild.Name != "entity")
                throw new NotImplementedException("FiVES does not support creation of nodes of type "
                    + receivedDocument.FirstChild.Name);

            return CreateEntityFromXML(receivedDocument.FirstChild);
        }

        public XmlElement UpdateEntity(string requestPath, string requestBody)
        {
            if (requestPath.Length == 0)
                throw new FormatException("Invalid request: need GUID of object to update");

            if (!requestPath.Contains('/'))
                return OverrideEntity(requestPath, requestBody);

            return HandleEntityComponentUpdate(requestPath, requestBody);
        }

        private XmlElement CreateEntityFromXML(XmlNode entityNode)
        {
            var receivedGuid = entityNode.Attributes["guid"];
            if (receivedGuid != null && !World.Instance.ContainsEntity(new Guid(receivedGuid.Value)))
                UpdatedEntity = new Entity(new Guid(entityNode.Attributes["guid"].Value), World.Instance.ID);
            else
                UpdatedEntity = new Entity();

            foreach (XmlNode component in entityNode.ChildNodes)
            {
                ApplyComponent(component);
            }

            World.Instance.Add(UpdatedEntity);
            return new SceneWriter(ResponseDocument).WriteEntity(UpdatedEntity);
        }

        private XmlElement OverrideEntity(string entityGuid, string requestBody)
        {
            World.Instance.Remove(World.Instance.FindEntity(entityGuid));
            return AddEntity(entityGuid, requestBody);
        }

        private XmlElement HandleEntityComponentUpdate(string requestPath, string requestBody)
        {
            string entityGuid = PathParser.Instance.GetFirstPathObject(requestPath);

            UpdatedEntity = World.Instance.FindEntity(entityGuid);
            string remainingPath = PathParser.Instance.GetRemainingPath(requestPath);
            UpdateEntityComponent(remainingPath, requestBody);

            return new SceneWriter(ResponseDocument).WriteEntity(UpdatedEntity);
        }

        private void UpdateEntityComponent(string remainingPath, string requestBody)
        {
            if (remainingPath.Contains('/') || remainingPath.Contains('?'))
                throw new NotImplementedException
                    ("FiVES currently does not support immediate attribute update via query syntax");
            XmlDocument componentDocument = new XmlDocument();
            componentDocument.LoadXml(requestBody);
            string xmlObjectName = componentDocument.FirstChild.Attributes["name"].Value;
            if (remainingPath != xmlObjectName)
                throw new FormatException("Request body contains different component name than requested resource:\n"
                    + "request: /" + remainingPath + " , resource: <component name='" + xmlObjectName + "'>");
            ApplyComponent(componentDocument.FirstChild);
        }

        private void ApplyComponent(XmlNode componentNode)
        {
            string componentName = componentNode.Attributes["name"].Value;
            foreach (XmlNode attribute in componentNode.ChildNodes)
            {
                SetAttributeValue(componentName, attribute);
            }
        }

        private void SetAttributeValue(string componentName, XmlNode attributeNode)
        {
            string attributeName = attributeNode.Attributes["name"].Value;
            Type attributeType = UpdatedEntity[componentName][attributeName].Type;
            var attributeValue = serializer.DeserializeObject(attributeNode.Attributes["value"].Value);
            UpdatedEntity[componentName][attributeName].Suggest(attributeValue);
        }

        private Vector parseVector(string vectorString)
        {
            string[] elements = getComplexTypeComponents(vectorString);
            return new Vector(float.Parse(elements[0]), float.Parse(elements[1]), float.Parse(elements[2]));
        }

        private Quat parseQuat(string quatString)
        {
            string[] elements = getComplexTypeComponents(quatString);
            return new Quat(
                float.Parse(elements[0]),
                float.Parse(elements[1]),
                float.Parse(elements[2]),
                float.Parse(elements[3])
            );
        }

        private AxisAngle parseAxisAngle(string axisAngleString)
        {
            string vectorString = getVectorStringPart(axisAngleString);
            string angleValue = getAngleStringPart(axisAngleString);
            return new AxisAngle(parseVector(vectorString), float.Parse(angleValue));
        }

        private string getVectorStringPart(string axisAngleString)
        {
            int vectorStart = axisAngleString.IndexOf("vector(");
            int vectorEnd = axisAngleString.IndexOf(")"); // first closing bracket, determines end of vector string
            int vectorLength = vectorEnd - vectorStart + 1; // we want to capture the closing bracket of vector as well
            return axisAngleString.Substring(vectorStart, vectorLength);
        }

        private string getAngleStringPart(string axisAngleString)
        {
            int commaIndex = axisAngleString.IndexOf(',');
            int lastValueLength = axisAngleString.LastIndexOf(')') - commaIndex - 1;
            return axisAngleString.Substring(commaIndex + 1, lastValueLength);
        }

        private string[] getComplexTypeComponents(string complexType)
        {
            int openBracket = complexType.IndexOf('(');
            int closingBracket = complexType.IndexOf(')');
            return complexType.Substring(openBracket + 1, closingBracket - openBracket - 1)
                .Trim().Split(' ');
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
    }
}
