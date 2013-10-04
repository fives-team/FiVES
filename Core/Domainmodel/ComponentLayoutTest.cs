using System;
using NUnit.Framework;
using FIVES;
using System.Collections.Generic;

namespace FIVES
{
    [TestFixture()]
    public class ComponentLayoutTest
    {
        public ComponentLayoutTest ()
        {
        }

        [Test()]
        public void DifferentEntriesShouldBeNotEqual()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            layout1.AddAttribute <int>("attribute1");
            layout1.AddAttribute <float> ("attribute2");

            ComponentLayout layout2 = new ComponentLayout ();
            layout1.AddAttribute <int>("attribute1");
            layout1.AddAttribute <int> ("attribute2");

            ComponentLayout layout3 = new ComponentLayout ();
            layout1.AddAttribute <int>("attribute1");
            layout1.AddAttribute <float> ("attribute3");

            Assert.IsFalse (layout1 == layout2);
            Assert.IsFalse (layout1 == layout3);

            Assert.IsTrue (layout1 != layout2);
            Assert.IsTrue (layout1 != layout3);
        }

        [Test()]
        public void SameEntriesShouldBeEqual()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            layout1.AddAttribute <int>("attribute1");
            layout1.AddAttribute <float> ("attribute2");

            ComponentLayout layout2 = new ComponentLayout ();
            layout2.AddAttribute <int>("attribute1");
            layout2.AddAttribute <float> ("attribute2");
          
            Assert.IsTrue (layout1 == layout2);
            Assert.IsFalse (layout1 != layout2);
        }

        [Test()]
        public void ShouldThrowExceptionOnIncorrectDefaultValueType()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            Assert.Throws<ArgumentException>(delegate { layout1.AddAttribute<int>("attribute1", 3.14f); });
            Assert.Throws<ArgumentException>(delegate { layout1.AddAttribute<float>("attribute2", 42); });
            Assert.Throws<ArgumentException>(delegate { layout1.AddAttribute<long>("attribute3", 12345m); });
        }

        [Test()]
        public void ShouldSupportNullAsDefaultValueTypeForNullableTypesAndThrowExceptionForNonNullableTypes()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            Assert.Throws<ArgumentException>(delegate { layout1.AddAttribute<int>("attribute1", null); });
            layout1.AddAttribute<string>("attribute2", null);
        }
    }
}

