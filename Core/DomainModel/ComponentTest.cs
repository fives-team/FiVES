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
        Entity parent;  // TODO: mock
        Component component;
        

        [SetUp()]
        public void Init()
        {
            // Set up definition.
            definition = new ComponentDefinition("test-component", 1);
            definition.AddAttribute<string>("a", "a_value");
            definition.AddAttribute<float>("b", 3.14f);

            // Set up parent.
            parent = new Entity();

            // Create component.
            component = new Component(definition, parent);
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
        public void ShouldRememberParent()
        {
            Assert.AreEqual(component.Parent, parent);
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

        [Test()]
        public void ShouldUpgradeAttributes()
        {
            component["b"] = 2f;

            // Create new definition
            ComponentDefinition newDefinition = new ComponentDefinition("test-component", 2); // TODO: mock
            newDefinition.AddAttribute<string>("a2", "");
            newDefinition.AddAttribute<float>("b", 0.0f);

            component.Upgrade(newDefinition, (comp1, comp2) => { 
                comp2["a2"] = comp1["a"]; 
                comp2["b"] = 1 / (float)comp1["b"]; 
            });

            Assert.AreEqual(component["a2"], "a_value");
            Assert.AreEqual(component["b"], 0.5f);
        }
    }
}
