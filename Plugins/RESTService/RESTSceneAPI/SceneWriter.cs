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
    public class SceneWriter
    {
        private XmlDocument ResponseDocument;

        public SceneWriter(XmlDocument responseDocument)
        {
            ResponseDocument = responseDocument;
        }

        public XmlElement WriteScene()
        {
            XmlElement sceneElement = ResponseDocument.CreateElement("scene");
            sceneElement.SetAttribute("guid", World.Instance.ID.ToString());
            foreach (var entity in World.Instance)
            {
                sceneElement.AppendChild(WriteEntity(entity));
            }
            return sceneElement;
        }

        public XmlElement WriteEntity(Entity entity)
        {
            var entityElement = ResponseDocument.CreateElement("entity");
            entityElement.SetAttribute("guid", entity.Guid.ToString());
            entityElement.SetAttribute("owner", entity.Owner.ToString());
            foreach (var component in entity.Components)
            {
                entityElement.AppendChild(WriteComponent(entity, component.Name));
            }
            return entityElement;
        }

        public XmlElement WriteComponent(Entity entity, string componentName)
        {
            XmlElement componentElement = ResponseDocument.CreateElement("component");
            var component = entity[componentName].Definition;
            foreach (var attribute in component.AttributeDefinitions)
            {
                componentElement.AppendChild(WriteAttribute(entity, componentName, attribute.Name));
            }
            componentElement.SetAttribute("name", componentName);
            return componentElement;
        }

        public XmlElement WriteAttribute(Entity entity, string componentName, string attributeName)
        {
            XmlElement AttributeElement = ResponseDocument.CreateElement("attribute");
            AttributeElement.SetAttribute("name", attributeName);
            AttributeElement.SetAttribute("value", WriteAttributeValue(entity, componentName, attributeName));
            return AttributeElement;
        }

        public string WriteAttributeValue(Entity entity, string componentName, string attributeName)
        {
            if (entity[componentName][attributeName].Value != null)
                return serializer.Serialize(entity[componentName][attributeName].Value);
            else
                return "null";
        }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
    }
}
