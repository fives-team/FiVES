﻿// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FIVES
{
    [TestFixture()]
    public class ComponentTest
    {
        ComponentDefinition definition; // TODO: mock
        Entity containingEntity;  // TODO: mock
        Component component;

        [SetUp()]
        public void Init()
        {
            // Set up definition.
            definition = new ComponentDefinition("test-component");
            definition.AddAttribute<string>("a", "a_value");
            definition.AddAttribute<float>("b", 3.14f);

            // Set up containing entity.
            containingEntity = new Entity();

            // Create component.
            component = new Component(definition, containingEntity);
        }

        [Test()]
        public void ShouldInitializeGuid()
        {
            Assert.IsNotNull(component.Guid);
            Assert.AreNotEqual(component.Guid, Guid.Empty);
        }

        [Test()]
        public void ShouldRememberDefinition()
        {
            Assert.AreEqual(component.Definition, definition);
        }

        [Test()]
        public void ShouldRememberContainingEntity()
        {
            Assert.AreEqual(component.ContainingEntity, containingEntity);
        }

        [Test()]
        public void ShouldAssignAndRetrieveValidAttributes()
        {
            component["a"] = "a_new_value";
            component["b"] = 2.71828f;

            Assert.AreEqual(component["a"], "a_new_value");
            Assert.AreEqual(component["b"], 2.71828f);
        }


        [Test()]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ShouldFailToRetrieveUndefinedAttribute()
        {
            object value = component["c"];
        }

        [Test()]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ShouldFailToAssignUndefinedAttribute()
        {
            component["c"] = false;
        }

        [Test()]
        [ExpectedException(typeof(AttributeAssignmentException))]
        public void ShouldFailToAssignValueWithInvalidType()
        {
            component["b"] = 3.14; // double assigned to float
        }

        [Test()]
        public void ShouldCreateAttributesAccordingToDefinition()
        {
            object a = component["a"];
            object b = component["b"];
            Assert.AreEqual(a.GetType(), typeof(string));
            Assert.AreEqual(b.GetType(), typeof(float));
            Assert.AreEqual(a, "a_value");
            Assert.AreEqual(b, 3.14f);
        }
    }
}
