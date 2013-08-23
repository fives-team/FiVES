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
            layout1.attributes = new Dictionary<string, Type> ();
            layout1.attributes ["attribute1"] = typeof(int);
            layout1.attributes ["attribute2"] = typeof(float);

            ComponentLayout layout2 = new ComponentLayout ();
            layout2.attributes = new Dictionary<string, Type> ();
            layout2.attributes ["attribute1"] = typeof(int);
            layout2.attributes ["attribute2"] = typeof(int);

            ComponentLayout layout3 = new ComponentLayout ();
            layout3.attributes = new Dictionary<string, Type> ();
            layout3.attributes ["attribute1"] = typeof(int);
            layout3.attributes ["attribute3"] = typeof(float);

            Assert.IsFalse (layout1 == layout2);
            Assert.IsFalse (layout1 == layout3);

            Assert.IsTrue (layout1 != layout2);
            Assert.IsTrue (layout1 != layout3);
        }

        [Test()]
        public void sameEntriesShouldBeEqual()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            layout1.attributes = new Dictionary<string, Type> ();
            layout1.attributes ["attribute1"] = typeof(int);
            layout1.attributes ["attribute2"] = typeof(float);

            ComponentLayout layout2 = new ComponentLayout ();
            layout2.attributes = new Dictionary<string, Type> ();
            layout2.attributes ["attribute1"] = typeof(int);
            layout2.attributes ["attribute2"] = typeof(float);
          
            Assert.IsTrue (layout1 == layout2);
            Assert.IsFalse (layout1 != layout2);
        }
    }
}

