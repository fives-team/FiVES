using System;
using NUnit.Framework;
using FIVES;

namespace ServerSyncPlugin
{
    [TestFixture]
    public class AttributeSyncInfoTest
    {
        [Test]
        public void ShouldCorrectlyResolveSyncConflicts()
        {
            var guid1 = Guid.Parse("38d6f8c5-ded5-405a-9f7d-80ebd36d7a26");
            var guid2 = Guid.Parse("e0021595-ead4-4a70-825b-749175c0d9b9");

            var info1 = new AttributeSyncInfo(guid2, 1);
            var info2 = new AttributeSyncInfo(guid2, 2);
            var info3 = new AttributeSyncInfo(guid1, 3);

            info3.LastTimestamp = info1.LastTimestamp;

            Assert.IsFalse(info2.Sync(info1));
            Assert.AreEqual(2, info2.LastValue);

            Assert.IsTrue(info3.Sync(info1));
            Assert.AreEqual(1, info3.LastValue);
        }
    }
}

