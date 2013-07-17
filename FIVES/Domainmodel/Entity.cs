using System;
using System.Collections.Generic;

namespace FIVES
{
	public class Entity
	{
		private IDictionary<string, Component> components { get; set; }

		public Entity ()
		{
			this.components = new Dictionary<string, Component>();
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

