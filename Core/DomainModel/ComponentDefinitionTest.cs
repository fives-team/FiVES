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
    public class ComponentDefinitionTest
    {
        ComponentDefinition definition;

        [SetUp()]
        public void Init()
        {
            definition = new ComponentDefinition("test-name");
        }

        [Test()]
        public void ShouldAddAttributes()
        {
            definition.AddAttribute<int>("a", 42);
            definition.AddAttribute<float>("b");
        }

        [Test()]
        public void ShouldRetrieveCollectionOfAttributes()
        {
            definition.AddAttribute<int>("a", 42);

            Assert.AreEqual(1, definition.AttributeDefinitions.Count);
            var enumerator = definition.AttributeDefinitions.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("a", enumerator.Current.Name);
            Assert.AreEqual(42, enumerator.Current.DefaultValue);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test()]
        public void ShouldAddAndRetrieveCorrectAttributesViaIndexedProperty()
        {
            definition.AddAttribute<int>("a", 42);
            definition.AddAttribute<float>("b");

            Assert.AreEqual("a", definition["a"].Name);
            Assert.AreEqual(typeof(int), definition["a"].Type);
            Assert.AreEqual(42, definition["a"].DefaultValue);

            Assert.AreEqual("b", definition["b"].Name);
            Assert.AreEqual(typeof(float), definition["b"].Type);
            Assert.AreEqual(default(float), definition["b"].DefaultValue);
        }

        [Test()]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ShouldFailToAccessNonExistingAttributeViaIndexedProperty()
        {
            var attr = definition["non-existing-attribute"];
        }

        [Test()]
        public void ShouldCorrectlyDetermineIfComponentContainsAnAttributeDefinitionByName()
        {
            definition.AddAttribute<int>("a", 42);
            definition.AddAttribute<float>("b");

            Assert.IsTrue(definition.ContainsAttributeDefinition("a"));
            Assert.IsTrue(definition.ContainsAttributeDefinition("b"));
            Assert.IsFalse(definition.ContainsAttributeDefinition("c"));
            Assert.IsFalse(definition.ContainsAttributeDefinition(""));
        }
    }
}
