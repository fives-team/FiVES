using System;

namespace FIVES
{
	public class Attribute
	{
		public string type { get; set; }
		public string value { get; set; }

		public Attribute()
		{
		}

		public Attribute(string type, string value)
		{
			this.type = type;
			this.value = value;
		}

		public void setValue<T>(T value)
		{
			string typeAsString = value.GetType().ToString();
			string valueAsString = value.ToString();
			this.type = typeAsString;
			this.value = valueAsString;
		}
	}
}

