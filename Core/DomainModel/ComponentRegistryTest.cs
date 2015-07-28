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
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES
{
    [TestFixture()]
    public class ComponentRegistryTest
    {
        // TODO: mock ComponentDefinition
        ComponentRegistry registry;
        ComponentDefinition definition;

        [SetUp()]
        public void Init()
        {
            registry = new ComponentRegistry();
            definition = new ComponentDefinition("test2");
            definition.AddAttribute<int>("a", 42);
        }

        [Test()]
        public void ShouldRetrieveCollectionOfRegisteredComponents()  // also checks for Register
        {
            registry.Register(definition);
            Assert.AreEqual(1, registry.RegisteredComponents.Count);
            var enumerator = registry.RegisteredComponents.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("test2", enumerator.Current.Name);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test()]
        [ExpectedException(typeof(ComponentRegistrationException))]
        public void ShouldFailToRegisterComponentTwice()
        {
            registry.Register(definition);
            registry.Register(definition);
        }
    }
}
