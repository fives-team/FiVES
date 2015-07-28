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
    public class ReadOnlyAttributeDefinitionTest
    {
        ReadOnlyAttributeDefinition definition;

        [SetUp()]
        public void Init()
        {
            definition = new ReadOnlyAttributeDefinition("test-name", typeof(int), 42, Guid.NewGuid());
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
