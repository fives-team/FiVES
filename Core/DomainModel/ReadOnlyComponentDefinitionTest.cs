// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
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
