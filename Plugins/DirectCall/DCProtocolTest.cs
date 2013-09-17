using NUnit.Framework;
using System;
using Moq;
using KIARA;

namespace DirectCall
{
    [TestFixture()]
    public class DCProtocolTest
    {
        public interface IHandlers {
            void Function1(int x);
        }

        private DCProtocol protocol;
        private Mock<IHandlers> mockHandlers;

        [SetUp()]
        public void Init()
        {
            protocol = new DCProtocol();
            mockHandlers = new Mock<IHandlers>();
        }

        [Test()]
        public void ShouldCallRegisteredFunctions()
        {
            protocol.RegisterHandler("function1", (Action<int>)mockHandlers.Object.Function1);
            protocol.CallFunc("function1", 42);
            mockHandlers.Verify(h => h.Function1(42), Times.Once());
        }

        [Test()]
        public void ShouldFailToCallUnregisteredFunctions()
        {
            Assert.Throws<Error>(() => protocol.CallFunc("unregisteredFunc"));
        }

        [Test()]
        public void ShouldFailToReregisterFunction()
        {
            protocol.RegisterHandler("function1", (Action<int>)mockHandlers.Object.Function1);
            Assert.Throws<Error>(
                () => protocol.RegisterHandler("function1", (Action<int>)mockHandlers.Object.Function1));
        }

        // TODO: Should process IDL (when implemented).
    }
}

