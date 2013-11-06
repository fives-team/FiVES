using NUnit.Framework;
using System;

namespace KIARA
{
    [TestFixture()]
    public class ServiceTest
    {
        class TestService : Service {
            public TestService(Context context) : base(context) {}
        }

        [Test()]
        public void ShouldStoreAssociatedContext()
        {
            Context context = new Context();
            TestService service = new TestService(context);
            Assert.AreEqual(context, service.context);
        }
    }
}

