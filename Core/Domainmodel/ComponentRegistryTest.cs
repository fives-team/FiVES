
using NUnit.Framework;
using System;

namespace FIVES
{
    [TestFixture()]
    public class ComponentRegistryTest
    {
        ComponentLayout layout = new ComponentLayout();
        ComponentRegistry registry;
        string name = "myComponent";

        public ComponentRegistryTest() {
            layout.addAttribute<int>("i");
            layout.addAttribute<float> ("f");
            layout.addAttribute<string> ("s");
            layout.addAttribute<bool>("b");
        }

        [SetUp()]
        public void init() {
            registry = new ComponentRegistry();
        }

        [Test()]
        public void shouldDefineComponentsWithoutException()
        {
            registry.defineComponent(name, Guid.NewGuid(), layout);
        }

        [Test()]
        public void shouldNotThrowExceptionWhenRedefiningSameComponent()
        {
            Guid owner = Guid.NewGuid();
            registry.defineComponent(name, owner, layout);
            registry.defineComponent(name, owner, layout);
        }

        [Test()]
        public void shouldThrowExceptionWhenRedefiningComponentWithDifferentLayout()
        {
            Guid owner = Guid.NewGuid();

            ComponentLayout otherLayout = new ComponentLayout();
            otherLayout.addAttribute<bool>("a");

            registry.defineComponent(name, owner, layout);
            Assert.Throws<ComponentAlreadyDefinedException>(
                delegate() { registry.defineComponent(name, owner, otherLayout); } );
        }

        [Test()]
        public void shouldThrowExceptionWhenRedefiningComponentWithDifferentOwner()
        {
            registry.defineComponent(name, Guid.NewGuid(), layout);
            Assert.Throws<ComponentAlreadyDefinedException>(
                delegate() { registry.defineComponent(name, Guid.NewGuid(), layout); } );
            
        }

        [Test()]
        public void shouldFailToConstructUndefinedComponent() {
            Assert.Throws<ComponentIsNotDefinedException>(
                delegate() { registry.getComponentInstance("foobar"); } );
        }

        [Test()]
        public void shouldCreateDefinedLayoutAttributesInNewComponents() 
        {
            registry.defineComponent(name, Guid.NewGuid(), layout);
            dynamic c = registry.getComponentInstance(name);
            Assert.AreEqual(c.i, default(int));
            Assert.AreEqual(c.f, default(float));
            Assert.AreEqual(c.s, default(string));
            Assert.AreEqual(c.b, default(bool));
        }

        [Test()]
        public void shouldNotCreateUndefinedAttributesInNewComponents() 
        {
            registry.defineComponent(name, Guid.NewGuid(), layout);
            dynamic c = registry.getComponentInstance(name);
            Assert.Throws<AttributeIsNotDefinedException>(
                delegate() { object result = c.foobar; } );
        }
    }
}

