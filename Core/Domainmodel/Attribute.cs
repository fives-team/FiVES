using System;

namespace FIVES
{
    /// <summary>
    /// Attribute type.
    /// </summary>
    public enum AttributeType {
        INT,
        FLOAT,
        STRING,
        BOOL
    }

    /// <summary>
    /// Typed attribute of a component.
    /// </summary>
    internal class Attribute
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        private Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the Attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        public AttributeType type { get; set; }

        /// <summary>
        /// Gets or sets the value of the Attribute.
        /// </summary>
        /// <value>The value of the Attribute.</value>
        public object value { get; set; }

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
        public Attribute(AttributeType type, object value)
        {
            this.type = type;
            this.value = value;
        }
    }
}

