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
            layout1.addAttribute <int>("attribute1");
            layout1.addAttribute <float> ("attribute2");

            ComponentLayout layout2 = new ComponentLayout ();
            layout1.addAttribute <int>("attribute1");
            layout1.addAttribute <int> ("attribute2");

            ComponentLayout layout3 = new ComponentLayout ();
            layout1.addAttribute <int>("attribute1");
            layout1.addAttribute <float> ("attribute3");

            Assert.IsFalse (layout1 == layout2);
            Assert.IsFalse (layout1 == layout3);

            Assert.IsTrue (layout1 != layout2);
            Assert.IsTrue (layout1 != layout3);
        }

        [Test()]
        public void sameEntriesShouldBeEqual()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            layout1.addAttribute <int>("attribute1");
            layout1.addAttribute <float> ("attribute2");

            ComponentLayout layout2 = new ComponentLayout ();
            layout2.addAttribute <int>("attribute1");
            layout2.addAttribute <float> ("attribute2");
          
            Assert.IsTrue (layout1 == layout2);
            Assert.IsFalse (layout1 != layout2);
        }
    }
}

