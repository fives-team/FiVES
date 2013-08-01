
using System;
using System.Collections.Generic;

namespace FIVES
{
    // Represents an attribute layout for the component. Use as following:
    //   layout = new ComponentLayout();
    //   layout["attrA"] = AttributeType.INT;
    //   layout["attrB"] = AttributeType.FLOAT;
    //   layout["attrC"] = AttributeType.STRING;
    public class ComponentLayout
    {
        public ComponentLayout() {
            this.attributes = new Dictionary<string, AttributeType> ();
        }
        public AttributeType this [string name] 
        {
            get { return attributes[name]; }
            set { attributes[name] = value; }
        }

        // We need to access this internally to be able to iterate over the list of the attributes when constructing a 
        // new component in ComponentRegistry::createComponent.
        private Guid Id { get; set; }
        internal IDictionary<string, AttributeType> attributes { get; set; }
    }
}

