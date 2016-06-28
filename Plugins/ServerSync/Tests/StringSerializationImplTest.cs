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
using System.Runtime.Serialization;

namespace ServerSyncPlugin
{
    [TestFixture]
    public class StringSerializationTest
    {
        [Serializable]
        class TestDoI : IDomainOfInterest
        {
            public int Param { get; private set; }

            public TestDoI(int param)
            {
                Param = param;
            }

            public bool IsInterestedInEntity(FIVES.Entity entity)
            {
                return false;
            }

            public bool IsInterestedInAttributeChange(FIVES.Entity entity, string componentName, string attributeName)
            {
                return false;
            }

#pragma warning disable 67
            public event System.EventHandler Changed;
#pragma warning restore 67

            public TestDoI(SerializationInfo info, StreamingContext context)
            {
                Param = info.GetInt32("param");
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("param", Param);
            }
        }

        [Test]
        public void ShouldDeserializeSerializedObjectsCorrectly()
        {
            var testDoI = new TestDoI(42);

            var ss = new StringSerializationImpl();
            string serializedDoI = ss.SerializeObject<IDomainOfInterest>(testDoI);
            object deserializedDoI = ss.DeserializeObject<IDomainOfInterest>(serializedDoI);

            Assert.IsTrue(deserializedDoI is TestDoI);
            Assert.AreEqual(testDoI.Param, (deserializedDoI as TestDoI).Param);
        }
    }
}
