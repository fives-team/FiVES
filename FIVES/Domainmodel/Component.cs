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
			this.checkForMatchingTypes (attribute, AttributeType.INT);
			return Convert.ToInt32(attribute.value);
		}

		public float getFloatAttribute(string name) {
			Attribute attribute = this.attributes [name];
			this.checkForMatchingTypes (attribute,AttributeType.FLOAT);
			return Convert.ToSingle(attribute.value);
		}

		public string getStringAttribute(string name) {
			Attribute attribute = this.attributes [name];
			this.checkForMatchingTypes (attribute, AttributeType.STRING);
			return (string)attribute.value;
		}

		public bool getBoolAttribute(string name) {
			Attribute attribute = this.attributes [name];
			this.checkForMatchingTypes (attribute, AttributeType.BOOL);
			return Convert.ToBoolean(attribute.value);
		}
		#endregion

		private void checkForMatchingTypes(Attribute requestedAttribute, AttributeType requestedType) {
			AttributeType attributeType = requestedAttribute.type;
			if (attributeType != requestedType)
			{
				throw new AttributeTypeMismatchException ("Error while retrieving Attribute value - Requested  " + requestedType.ToString() + " but Attribute was of type " + attributeType.ToString());
			}
		}

		private void setAttribute<T>(string name, T value, AttributeType type) {
			Attribute newAttribute = new Attribute (type, value.ToString ());
			this.attributes.Add (name, newAttribute);
		}
	}
}
