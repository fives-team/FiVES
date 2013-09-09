using NUnit.Framework;
using System;

namespace KIARA
{
    [TestFixture()]
    public class ContextFactoryTest
    {
        [Test()]
        public void shouldCreateNewContexts()
        {
            Assert.NotNull(ContextFactory.getContext("test-context-1"));
        }

        [Test()]
        public void shouldReturnSameContextForSameName()
        {
            Context ctx = ContextFactory.getContext("test-context-2");
            Assert.AreEqual(ctx, ContextFactory.getContext("test-context-2"));
        }
    }
}

