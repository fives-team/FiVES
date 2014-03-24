using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using V8.Net;

namespace ScriptingPlugin
{
    /// <summary>
    /// This wrapper adds support for the indexed properties with a single string argument.
    /// </summary>
    class V8NetObjectWrapper : ObjectBinder
    {
        public static V8NetObjectWrapper Wrap(V8Engine engine, object obj)
        {
            var objType = obj.GetType();
            var typeBinder = engine.RegisterType(objType, null, true, ScriptMemberSecurity.Locked);
            return typeBinder.CreateObject<V8NetObjectWrapper, object>(obj);
        }

        public override InternalHandle NamedPropertyGetter(ref string propertyName)
        {
            if (!searchedStringIndexers)
                FindStringIndexers();

            InternalHandle handle = base.NamedPropertyGetter(ref propertyName);

            if (handle.IsEmpty && namedIndexerGetter != null)
            {
                object value = namedIndexerGetter.Invoke(Object, new object[] { propertyName });
                if (value != null && value.GetType().IsClass)
                    value = V8NetObjectWrapper.Wrap(Engine, value);
                var v = Engine.CreateValue(value, true, ScriptMemberSecurity.Locked);
                return v;
            }

            return handle;
        }

        public override InternalHandle NamedPropertySetter(ref string propertyName, InternalHandle value, V8PropertyAttributes attributes = V8PropertyAttributes.Undefined)
        {
            if (!searchedStringIndexers)
                FindStringIndexers();

            InternalHandle handle = base.NamedPropertySetter(ref propertyName, value, attributes);

            if (handle.IsEmpty && namedIndexerSetter != null)
            {
                object convertedValue = new ArgInfo(value, null, namedPropertyType).ValueOrDefault;
                namedIndexerSetter.Invoke(Object, new object[] { propertyName, convertedValue });
                return value;
            }

            return handle;
        }

        private void FindStringIndexers()
        {
            // Find out how default properties are called in this object's type. By default they are called "Item".
            string indexerName = "Item";
            object[] defaultMemberAttributes = ObjectType.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
            if (defaultMemberAttributes.Length > 0)
            {
                var attr = defaultMemberAttributes[defaultMemberAttributes.Length - 1] as DefaultMemberAttribute;
                indexerName = attr.MemberName;
            }

            // Find out if there is an indexed property with a single string argument and check if it has getter and 
            // setter.
            var stringIndexer = ObjectType.GetProperty(indexerName, new Type[] { typeof(string) });
            if (stringIndexer != null)
            {
                namedPropertyType = stringIndexer.PropertyType;
                if (stringIndexer.CanRead)
                    namedIndexerGetter = stringIndexer.GetGetMethod();
                if (stringIndexer.CanWrite)
                    namedIndexerSetter = stringIndexer.GetSetMethod();
            }

            searchedStringIndexers = true;
        }

        bool searchedStringIndexers = false;
        Type namedPropertyType = null;
        MethodInfo namedIndexerGetter = null;
        MethodInfo namedIndexerSetter = null;
    }
}
