using System.Collections.Generic;

namespace SIXstrichLDPlugin
{
    public static class LD
    {
        public static string xsdURI = "http://www.w3.org/2001/XMLSchema#";
        public static string schemaURI = "http://schema.org/";
        public static string entityURI = "http://www.dfki.de/fives/entity#";
        public static string componentURI = "http://www.dfki.de/fives/component#";
        public static string attributeURI = "http://www.dfki.de/fives/attribute#";

        public const string ID = "@id";
        public const string TYPE = "@type";
        public const string LIST = "@list";
        public const string CONTAINER = "@container";

        public const string URL = "schema:url";
        public const string ANY = "xsd:any";
        public const string INT = "xsd:int";
        public const string BYTE = "xsd:byte";
        public const string FLOAT = "xsd:float";
        public const string DOUBLE = "xsd:double";
        public const string STRING = "xsd:string";
        public const string BOOLEAN = "xsd:boolean";
        public const string CONTEXT = "@context";
    }

    public static class LDContext
    {
        private static Dictionary<string, string> getTypeDictForID(string id, string LDType)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(LD.ID, id);
            dict.Add(LD.TYPE, LDType);
            return dict;
        }

        private static Dictionary<string, string> getTypeIDDictForID(string id)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(LD.ID, id);
            dict.Add(LD.TYPE, LD.ID);
            return dict;
        private static string getLDType(Type type)
        {
            if (type.Equals(typeof(string)))
            {
                return LD.STRING;
            }
            else if (type.Equals(typeof(bool)))
            {
                return LD.BOOLEAN;
            }
            else if (type.Equals(typeof(byte)))
            {
                return LD.BYTE;
            }
            else if (type.Equals(typeof(int)))
            {
                return LD.INT;
            }
            else if (type.Equals(typeof(float)))
            {
                return LD.FLOAT;
            }
            else if (type.Equals(typeof(double)))
            {
                return LD.DOUBLE;
            }
            else
            {
                return LD.ANY;
            }
        }

        private static Dictionary<string, string> getContainerDictForID(string id)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(LD.ID, id);
            dict.Add(LD.CONTAINER, LD.LIST);
            return dict;
        }

        public static Dictionary<string, object> getEntityCollectionBase()
        {
            var collectionContext = new Dictionary<string, object>();
            var context = new Dictionary<string, object>();
            context.Add("schema", LD.schemaURI);
            context.Add("entities", getContainerDictForID(LD.URL));
            collectionContext.Add(LD.CONTEXT, context);
            return collectionContext;
        }

        public static Dictionary<string, object> getEntityBase()
        {
            var entityContext = new Dictionary<string, object>();
            var context = new Dictionary<string, object>();
            context.Add("xsd", LD.xsdURI);
            context.Add("schema", LD.schemaURI);
            context.Add("entity", LD.entityURI);
            context.Add("guid", getTypeDictForID("entity:guid", LD.STRING));
            context.Add("owner", getTypeDictForID("entity:owner", LD.STRING));
            context.Add("world", getTypeDictForID("entity:world", LD.ID));
            context.Add("components", getContainerDictForID(LD.URL));
            entityContext.Add(LD.CONTEXT, context);
            return entityContext;
        }

        public static Dictionary<string, object> getComponentBase()
        {
            var componentContext = new Dictionary<string, object>();
            var context = new Dictionary<string, object>();
            context.Add("xsd", LD.xsdURI);
            context.Add("schema", LD.schemaURI);
            context.Add("component", LD.componentURI);
            context.Add("name", getTypeDictForID("component:name", LD.STRING));
            context.Add("containingEntity", getTypeDictForID("component:containingEntity", LD.ID));
            context.Add("attributes", getContainerDictForID(LD.URL));
            componentContext.Add(LD.CONTEXT, context);
            return componentContext;
        }

        public static Dictionary<string, object> getAttributeBase()
        {
            var attributeContext = new Dictionary<string, object>();
            var context = new Dictionary<string, object>();
            context.Add("xsd", LD.xsdURI);
            context.Add("attribute", LD.attributeURI);
            context.Add("name", getTypeDictForID("attribute:name", LD.STRING));
            context.Add("parentComponent", getTypeDictForID("attribute:parentComponent", LD.TYPE));
            context.Add("currentValue", getTypeStringDictForID("attribute:currentValue"));
            attributeContext.Add(LD.CONTEXT, context);
            return attributeContext;
        }
    }
}
