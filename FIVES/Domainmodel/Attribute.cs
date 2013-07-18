using System;

namespace FIVES
{
	public class Attribute
	{
		public AttributeType type { get; set; }
		public object value { get; set; }

		public Attribute()
		{
		}

		public Attribute(AttributeType type, string value)
		{
			this.type = type;
			this.value = value;
		}
	}

	public enum AttributeType {
		INT,
		FLOAT,
		STRING,
		BOOL
	}
}

