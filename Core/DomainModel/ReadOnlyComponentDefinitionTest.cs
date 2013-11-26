using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    [TestFixture()]
    public class ReadOnlyComponentDefinitionTest
    {
        class ReadOnlyComponentDefinitionImpl : ReadOnlyComponentDefinition
        {
            public ReadOnlyComponentDefinitionImpl() : base("test-name", Guid.NewGuid())
            {
            }

            public override ReadOnlyCollection<ReadOnlyAttributeDefinition> AttributeDefinitions
            {
                get { throw new NotImplementedException(); }
            }

            public override ReadOnlyAttributeDefinition this[string attributeName]
            {
                get { throw new NotImplementedException(); }
            }

            public override bool ContainsAttributeDefinition(string attributeName)
            {
                throw new NotImplementedException();
            }
        }

        ReadOnlyComponentDefinitionImpl definition;

        [SetUp()]
        public void Init()
        {
            definition = new ReadOnlyComponentDefinitionImpl();
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
    }
}
