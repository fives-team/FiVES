using NUnit.Framework;
using System;
using Moq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJProtocolTest
    {
        private class WSJProtocolWrapper : WSJProtocol
        {
            public WSJProtocolWrapper(IWSJFuncCallFactory factory) : base(factory) {}

            public override void Send(string message)
            {
                sentMessages.Add(message);
            }

            public List<string> sentMessages = new List<string>();
        }

        public interface IHandlers {
            float testFunc(int i, string s);
        }

        WSJProtocolWrapper protocol;
        Mock<IWSJFuncCall> mockWSJFuncCall;
        Mock<IWSJFuncCallFactory> mockWSJFuncCallFactory;
        Mock<IHandlers> mockHandlers;

        [SetUp()]
        public void init()
        {
            mockWSJFuncCallFactory = new Mock<IWSJFuncCallFactory>();
            mockWSJFuncCall = new Mock<IWSJFuncCall>();
            mockWSJFuncCallFactory.Setup(f => f.construct()).Returns(mockWSJFuncCall.Object);
            mockHandlers = new Mock<IHandlers>();

            protocol = new WSJProtocolWrapper(mockWSJFuncCallFactory.Object);
        }

        [Test()]
        public void shouldCorrectlyFormatCallMessage()
        {
            protocol.callFunc("testFunc", 42, "test-string");
            Assert.AreEqual(protocol.sentMessages[0], "[\"call\",0,\"testFunc\",42,\"test-string\"]");
        }

        [Test()]
        public void shouldHandleSuccessCallReply()
        {
            protocol.callFunc("testFunc", 42, "test-string");
            protocol.handleMessage("['call-reply',0,true,3.14]");
            mockWSJFuncCall.Verify(c => c.handleSuccess(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void shouldHandleExceptionCallReply()
        {
            protocol.callFunc("testFunc", 42, "test-string");
            protocol.handleMessage("['call-reply',0,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.handleException(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void shouldFailAllActiveCallsWithErrorOnClose()
        {
            protocol.callFunc("testFunc1", 42, "test-string");
            protocol.callFunc("testFunc2", "foobar", 123);
            protocol.handleClose(SuperSocket.SocketBase.CloseReason.InternalError);

            mockWSJFuncCall.Verify(c => c.handleError(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test()]
        public void shouldProcessConcurrentCallsCorrectly()
        {
            protocol.callFunc("testFunc1", 42, "test-string");
            protocol.callFunc("testFunc2", "foobar", 123);
            protocol.handleMessage("['call-reply',0,true,'ret-val-1']");
            mockWSJFuncCall.Verify(c => c.handleSuccess(It.IsAny<JToken>()), Times.Once());
            protocol.callFunc("testFunc3");
            protocol.handleMessage("['call-reply',2,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.handleException(It.IsAny<JToken>()), Times.Once());
            protocol.handleMessage("['call-reply',1,true,'ret-val-2']");
            mockWSJFuncCall.Verify(c => c.handleSuccess(It.IsAny<JToken>()), Times.Exactly(2));
        }

        [Test()]
        public void shouldProcessCallReplyWithNoRetValueCorrectly()
        {
            protocol.callFunc("testFunc1", 42, "test-string");
            protocol.handleMessage("['call-reply',0,true]");
        }

        [Test()]
        public void shouldCorrectlyHandleRemoteCallRequestForRegisteredFunctionName()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            protocol.handleMessage("['call',0,'testFunc',42,'test-string']");
            mockHandlers.Verify(h => h.testFunc(42, "test-string"), Times.Once());
        }

        [Test()]
        public void shouldFailOnRemoteCallRequestForUnregisteredFunctionName()
        {
            Assert.Throws<UnregisteredMethod>(() => protocol.handleMessage("['call',0,'unknownFunc']"));
        }

        [Test()]
        public void shouldFailOnRemoteCallRequestWithInvalidNumberOfArgs()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            Assert.Throws<InvalidNumberOfArgs>(() => protocol.handleMessage("['call',0,'testFunc',42]"));
        }

        [Test()]
        public void shouldFailOnCallReplyWithUnknownCallID()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            Assert.Throws<UnknownCallID>(() => protocol.handleMessage("['call-reply',100,'testFunc',42,'foobar']"));
        }

        [Test()]
        public void shouldFailToReregisterHandlerForTheSameFunctionName()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            Assert.Throws<HandlerAlreadyRegistered>(
                () => protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc));
        }

        // TODO: Should process IDL (when implemented).
    }
}

