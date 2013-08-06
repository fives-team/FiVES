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
            void function1(int x);
        }

        private DCProtocol protocol;
        private Mock<IHandlers> mockHandlers;

        [SetUp()]
        public void init()
        {
            protocol = new DCProtocol();
            mockHandlers = new Mock<IHandlers>();
        }

        [Test()]
        public void shouldCallRegisteredFunctions()
        {
            protocol.registerHandler("function1", (Action<int>)mockHandlers.Object.function1);
            protocol.callFunc("function1", 42);
            mockHandlers.Verify(h => h.function1(42), Times.Once());
        }

        [Test()]
        public void shouldFailToCallUnregisteredFunctions()
        {
            Assert.Throws<Error>(() => protocol.callFunc("unregisteredFunc"));
        }

        [Test()]
        public void shouldFailToReregisterFunction()
        {
            protocol.registerHandler("function1", (Action<int>)mockHandlers.Object.function1);
            Assert.Throws<Error>(
                () => protocol.registerHandler("function1", (Action<int>)mockHandlers.Object.function1));
        }

        // TODO: Should process IDL (when implemented).
    }
}

