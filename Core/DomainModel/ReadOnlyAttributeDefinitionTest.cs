using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    [TestFixture()]
    public class ReadOnlyAttributeDefinitionTest
    {
        ReadOnlyAttributeDefinition definition;

        [SetUp()]
        public void Init()
        {
            definition = new ReadOnlyAttributeDefinition("test-name", typeof(int), 42);
        }

        [Test()]
        public void ShouldInitializeGuid()
        {
            Assert.IsNotNull(definition.Guid);
            Assert.AreNotEqual(Guid.Empty, definition.Guid);
        }

        [Test()]
        public void ShouldReturnCorrectName()
        {
            Assert.AreEqual("test-name", definition.Name);
        }

        [Test()]
        public void ShouldReturnCorrectType()
        {
            Assert.AreEqual(typeof(int), definition.Type);
        }

        [Test()]
        public void ShouldReturnCorrectDefaultValue()
        {
            Assert.AreEqual(42, definition.DefaultValue);
        }
    }
}
