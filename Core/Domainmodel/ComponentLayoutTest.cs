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
        public void differentEntriesShouldBeNotEqual()
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
        public void sameEntriesShouldBeEqual()
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
    }
}

