using NUnit.Framework;
using System;

namespace KIARA
{
    [TestFixture()]
    public class ContextFactoryTest
    {
        [Test()]
        public void ShouldCreateNewContexts()
        {
            Assert.NotNull(ContextFactory.GetContext("test-context-1"));
        }

        [Test()]
        public void ShouldReturnSameContextForSameName()
        {
            Context ctx = ContextFactory.GetContext("test-context-2");
            Assert.AreEqual(ctx, ContextFactory.GetContext("test-context-2"));
        }
    }
}

