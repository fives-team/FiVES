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
            string Function2();
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
        public void ShouldReregisterFunction()
        {
            protocol.RegisterHandler("function", (Action<int>)mockHandlers.Object.Function1);
            protocol.RegisterHandler("function", (Func<string>)mockHandlers.Object.Function2);
            protocol.CallFunc("function");
            mockHandlers.Verify(h => h.Function1(It.IsAny<int>()), Times.Never());
            mockHandlers.Verify(h => h.Function2(), Times.Once());
        }

        // TODO: Should process IDL (when implemented).
    }
}

