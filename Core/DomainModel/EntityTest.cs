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
    public class EntityTest
    {
        public interface IMockHandlers 
        {
            void CreatedComponent(object sender, ComponentEventArgs e);
            void ChangedAttribute(object sender, ChangedAttributeEventArgs e);
        }

        Entity entity;
        Mock<IMockHandlers> mockHandlers;
        Mock<IComponentRegistry> mockComponentRegistry;

        [SetUp()]
        public void Init()
        {
            // TODO: mock ComponentDefinition
            ComponentDefinition test = new ComponentDefinition("test");
            test.AddAttribute<int>("a", 42);
            test.AddAttribute<int?>("n", 5);

            mockComponentRegistry = new Mock<IComponentRegistry>();
            mockComponentRegistry.Setup(r => r.FindComponentDefinition("test")).Returns(test);

            entity = new Entity();
            entity.componentRegistry = mockComponentRegistry.Object;

            mockHandlers = new Mock<IMockHandlers>();
        }

        [Test()]
        public void ShouldConvertCompatibleDefaultValues()
        {
            ComponentDefinition def = new ComponentDefinition("defTypeTest");

            def.AddAttribute<double>("c", 42.0);
            def.AddAttribute<int?>("e", null);
            def.AddAttribute<int?>("f", 34);

            Assert.Catch<AttributeDefinitionException>(delegate {
                def.AddAttribute<double>("d", 42);
            });

            Assert.Catch<AttributeDefinitionException>(delegate {
                def.AddAttribute<double>("a", 42);
            });

            Assert.Catch<AttributeDefinitionException>(delegate {
                def.AddAttribute<double>("b", 42f);
            });

            Assert.Catch<AttributeDefinitionException>(delegate {
                def.AddAttribute<int>("g", 3.14);
            });
        }

        [Test()]
        public void ShouldInitializeGuid()
        {
            Assert.IsNotNull(entity.Guid);
            Assert.AreNotEqual(Guid.Empty, entity.Guid);
        }

        [Test()]
        public void ShouldCreateComponentsWhenAccessedAndCorrectVerifyIfEntityContainsAComponent()
        {
            Assert.IsFalse(entity.ContainsComponent("test"));
            entity["test"]["a"].Value = 24;
            Assert.IsTrue(entity.ContainsComponent("test"));
        }

        [Test()]
        [ExpectedException(typeof(ComponentAccessException))]
        public void ShouldFailToCreateUnregisteredComponents()
        {
            entity["unregistered-component"]["a"].Suggest(24);
        }

        [Test()]
        public void ShouldReturnCollectionOfComponents()
        {
            entity["test"]["a"].Value = 24;

            Assert.AreEqual(1, entity.Components.Count);
            var enumerator = entity.Components.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("test", enumerator.Current.Name);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test()]
        public void ShouldFireCreatedComponentEvents()
        {
            entity.CreatedComponent += mockHandlers.Object.CreatedComponent;

            entity["test"]["a"].Value = 24;

            mockHandlers.Verify(h => h.CreatedComponent(It.IsAny<object>(), 
                It.IsAny<ComponentEventArgs>()), Times.Once());
        }

        [Test()]
        public void ShouldFireChangedAttributeEvents()
        {
            entity.ChangedAttribute += mockHandlers.Object.ChangedAttribute;

            entity["test"]["a"].Value = 24;

            mockHandlers.Verify(h => h.ChangedAttribute(It.IsAny<object>(),
                It.IsAny<ChangedAttributeEventArgs>()), Times.Once());
        }

        [Test()]
        public void ShouldNotFireChangedAttributeEventsWhenTheValueIsNotChanged()
        {
            entity.ChangedAttribute += mockHandlers.Object.ChangedAttribute;

            entity["test"]["a"].Value = 42;

            mockHandlers.Verify(h => h.ChangedAttribute(It.IsAny<object>(),
                It.IsAny<ChangedAttributeEventArgs>()), Times.Never());
        }

        [Test()]
        public void ShouldHandleNullValuesForAttributesCorrectly()
        {
            entity.ChangedAttribute += mockHandlers.Object.ChangedAttribute;

            entity["test"]["n"].Value = null;
            entity["test"]["n"].Value = null; // shouldn't cause AttributeChanged event
            entity["test"]["n"].Value = 5;

            mockHandlers.Verify(h => h.ChangedAttribute(It.IsAny<object>(),
                It.IsAny<ChangedAttributeEventArgs>()), Times.Exactly(2));
        }
    }
}
