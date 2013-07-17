using System;
using System.Collections.Generic;

namespace FIVES
{
	public class Component
	{
		private IDictionary<string, Attribute> attributes { get; set; }

		public Component ()
		{
			this.attributes = new Dictionary<string, Attribute> ();
		}

		public void addAttribute<T>(string name, T value)
		{
			Attribute attributeToAdd = new Attribute ();
			attributeToAdd.setValue (value);
			this.attributes.Add (name, attributeToAdd);
		}

		public Attribute this [string index]
		{
			get 
			{
				return this.attributes [index];
			}
			set 
			{
				this.attributes [index] = value;
			}
		}

		public string getAttributeTypeAsString(string name)
		{
			return this.attributes[name].type;
		}

		public string getAttributeValueAsString(string name)
		{
			return this.attributes[name].value;
		}

	}
}
