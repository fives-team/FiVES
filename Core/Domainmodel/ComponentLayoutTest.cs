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
            layout1.attributes = new Dictionary<string, AttributeType> ();
            layout1.attributes ["attribute1"] = AttributeType.INT;
            layout1.attributes ["attribute2"] = AttributeType.FLOAT;

            ComponentLayout layout2 = new ComponentLayout ();
            layout2.attributes = new Dictionary<string, AttributeType> ();
            layout2.attributes ["attribute1"] = AttributeType.INT;
            layout2.attributes ["attribute2"] = AttributeType.INT;

            ComponentLayout layout3 = new ComponentLayout ();
            layout3.attributes = new Dictionary<string, AttributeType> ();
            layout3.attributes ["attribute1"] = AttributeType.INT;
            layout3.attributes ["attribute3"] = AttributeType.FLOAT;

            Assert.IsFalse (layout1 == layout2);
            Assert.IsFalse (layout1 == layout3);

            Assert.IsTrue (layout1 != layout2);
            Assert.IsTrue (layout1 != layout3);
        }

        [Test()]
        public void sameEntriesShouldBeEqual()
        {
            ComponentLayout layout1 = new ComponentLayout ();
            layout1.attributes = new Dictionary<string, AttributeType> ();
            layout1.attributes ["attribute1"] = AttributeType.INT;
            layout1.attributes ["attribute2"] = AttributeType.FLOAT;

            ComponentLayout layout2 = new ComponentLayout ();
            layout2.attributes = new Dictionary<string, AttributeType> ();
            layout2.attributes ["attribute1"] = AttributeType.INT;
            layout2.attributes ["attribute2"] = AttributeType.FLOAT;
          
            Assert.IsTrue (layout1 == layout2);
            Assert.IsFalse (layout1 != layout2);
        }
    }
}

