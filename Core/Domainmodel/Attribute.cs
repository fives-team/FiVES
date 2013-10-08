using System;

namespace FIVES
{
    /// <summary>
    /// Typed attribute of a component.
    /// </summary>
    internal class Attribute
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        internal Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the type of the Attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the value of the Attribute.
        /// </summary>
        /// <value>The value of the Attribute.</value>
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FIVES.Attribute"/> class.
        /// </summary>
        public Attribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FIVES.Attribute"/> class.
        /// </summary>
        /// <param name="type">Type the attribute should have</param>
        /// <param name="value">Value the attribute should have</param>
        public Attribute(Type type, object value)
        {
            this.Type = type;
            this.Value = value;
        }
    }
}

