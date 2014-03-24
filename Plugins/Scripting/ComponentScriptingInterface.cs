using FIVES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptingPlugin
{
    class ComponentScriptingInterface
    {
        public ComponentScriptingInterface(Component aComponent)
        {
            component = aComponent;
        }

        public object this[string attributeName]
        {
            get
            {
                try
                {
                    return component[attributeName];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }

            set
            {
                try
                {
                    component[attributeName] = value;
                }
                catch (KeyNotFoundException)
                {
                    // TODO: throw exception in the script
                }
                catch (InvalidCastException)
                {
                    // TODO: throw exception in the script
                }
                catch (OverflowException)
                {
                    // TODO: throw exception in the script
                }
                catch (FormatException)
                {
                    // TODO: throw exception in the script
                }
            }
        }

        public string toString()
        {
            return "[component " + component.Name + "]";
        }

        Component component;
    }
}
