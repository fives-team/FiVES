using System;
using System.Collections.Generic;

namespace FIVES
{
	public class Entity
	{
		private Dictionary<string, Component> components = new Dictionary<string, Component>();

		public Entity ()
		{
		}

		public Component this [string index]
		{
			get {
				return this.components [index];
			}
			set {
				this.components [index] = value;
			}
		}
	}
}

