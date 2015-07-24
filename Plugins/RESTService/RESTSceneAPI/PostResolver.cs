using FIVES;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RESTServicePlugin
{
    public class PostResolver
    {
        private XmlDocument ResponseDocument;
        private Entity AddedEntity;

        public PostResolver(XmlDocument responseDocumet)
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

        private XmlElement CreateEntityFromXML(XmlNode entityNode)
        {
            var receivedGuid = entityNode.Attributes["guid"];
            if (receivedGuid != null && !World.Instance.ContainsEntity(new Guid(receivedGuid.Value)))
                AddedEntity = new Entity(new Guid(entityNode.Attributes["guid"].Value), World.Instance.ID);
            else
                AddedEntity = new Entity();

            foreach (XmlNode component in entityNode.ChildNodes)
            {
                ApplyComponent(component);
            }

            World.Instance.Add(AddedEntity);
            return new SceneWriter(ResponseDocument).WriteEntity(AddedEntity);
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
            Type attributeType = AddedEntity[componentName][attributeName].Type;
            var attributeValue = parseAttributeValue (attributeNode.Attributes["value"].Value, attributeType);
            AddedEntity[componentName][attributeName].Suggest(attributeValue);
        }

        private object parseAttributeValue(string value, Type attributeType)
        {
            if (value.StartsWith("vector"))
                return parseVector(value);
            else if (value.StartsWith("quat"))
                return parseQuat(value);
            else if (attributeType == typeof(bool))
                return bool.Parse(value);
            else if (attributeType == typeof(string))
                return value;
            else if (attributeType == typeof(float))
                return float.Parse(value);
            else if (attributeType == typeof(int))
                return int.Parse(value);

            else throw new ArgumentException("FiVES currently does not support updates to attributes of Type "
                + attributeType.ToString() + " via REST");
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

        private string[] getComplexTypeComponents(string complexType)
        {
            int openBracket = complexType.IndexOf('(');
            int closingBracket = complexType.IndexOf(')');
            return complexType.Substring(openBracket + 1, closingBracket - openBracket - 1)
                .Trim().Split(' ');
        }
    }
}
