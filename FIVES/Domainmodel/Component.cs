using System;
using System.Collections.Generic;

namespace FIVES
{
	public class AttributeTypeMismatchException : System.Exception
	{
		public AttributeTypeMismatchException(){}
		public AttributeTypeMismatchException(string message){}
	}

	public class Component
	{
		private IDictionary<string, Attribute> attributes { get; set; }

		public Component ()
		{
			this.attributes = new Dictionary<string, Attribute> ();
		}

		#region Typed Attribute Setters
		public void setIntAttribute(string name, int value) {
			this.setAttribute (name, value, AttributeType.INT);
		}

		public void setFloatAttribute(string name, float value) {
			this.setAttribute (name, value, AttributeType.FLOAT);
		}

		public void setStringAttribute(string name, string value) {
			this.setAttribute (name, value, AttributeType.STRING);
		}

		public void setBoolAttribute(string name, bool value) {
			this.setAttribute (name, value, AttributeType.BOOL);
		}
		#endregion

		#region Typed Attribute Getters
		public int getIntAttribute(string name) {
			Attribute attribute = this.attributes [name];
			AttributeType attributeType = attribute.type;

			if (attributeType != AttributeType.INT)
			{
				throw new AttributeTypeMismatchException ("Error while retrieving Attribute value - Requested INT but Attribute was of type " + attributeType.ToString());
			}

			int outValue;
			int.TryParse(attribute.value, out outValue);

			return outValue;
		}

		public float getFloatAttribute(string name) {
			Attribute attribute = this.attributes [name];
			AttributeType attributeType = attribute.type;

			if (attributeType != AttributeType.FLOAT)
			{
				throw new AttributeTypeMismatchException ("Error while retrieving Attribute value - Requested FLOAT but Attribute was of type " + attributeType.ToString());
			}

			float outValue;
			float.TryParse(attribute.value, out outValue);

			return outValue;
		}

		public string getStringAttribute(string name) {
			Attribute attribute = this.attributes [name];
			AttributeType attributeType = attribute.type;

			if (attributeType != AttributeType.STRING)
			{
				throw new AttributeTypeMismatchException ("Error while retrieving Attribute value - Requested STRING but Attribute was of type " + attributeType.ToString());
			}

			return attribute.value;
		}

		public bool getBoolAttribute(string name) {
			Attribute attribute = this.attributes [name];
			AttributeType attributeType = attribute.type;

			if (attributeType != AttributeType.BOOL)
			{
				throw new AttributeTypeMismatchException ("Error while retrieving Attribute value - Requested BOOL but Attribute was of type " + attributeType.ToString());
			}

			bool outValue;
			bool.TryParse(attribute.value, out outValue);

			return outValue;
		}

		#endregion
		private void setAttribute<T>(string name, T value, AttributeType type) {
			Attribute newAttribute = new Attribute (type, value.ToString ());
			this.attributes.Add (name, newAttribute);
		}
	}
}
