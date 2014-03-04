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
