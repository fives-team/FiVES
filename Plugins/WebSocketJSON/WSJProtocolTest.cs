using NUnit.Framework;
using System;
using Moq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using KIARA;

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
            void testCallback(int i, FuncWrapper callback);
            void testCallback2(string a, Action<string> hello);
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
            Assert.AreEqual(protocol.sentMessages[0], "[\"call\",0,\"testFunc\",[],42,\"test-string\"]");
        }

        [Test()]
        public void shouldCorrectlyEncodeNativeCallbacksToTheMessage()
        {
            protocol.callFunc("testFunc", 42, "test-string", (Action)delegate() {});
            Assert.That(protocol.sentMessages[0],
                Is.StringMatching("\\[\"call\",0,\"testFunc\",\\[2\\],42,\"test-string\",\"[0-9a-feA-F\\-]+\"\\]"));
        }

        [Test()]
        public void shouldSendCallReplies()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            mockHandlers.Setup(h => h.testFunc(42, "test-string")).Returns(3.14f);
            protocol.handleMessage("['call',0,'testFunc',[],42,'test-string']");
            Assert.AreEqual(protocol.sentMessages[0], "[\"call-reply\",0,true,3.14]");
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
        public void shouldHandleRemoteCallbacksCorrectly()
        {
            protocol.registerHandler("testCallback", (Action<int, FuncWrapper>)mockHandlers.Object.testCallback);
            FuncWrapper generatedFuncWrapper = null;
            mockHandlers.Setup(h => h.testCallback(42, It.IsAny<FuncWrapper>()))
                .Callback((int i, FuncWrapper f) => generatedFuncWrapper = f);
            protocol.handleMessage("['call',0,'testCallback',[1],42,'99095a90-1997-11e3-8ffd-0800200c9a66']");
            mockHandlers.Verify(h => h.testCallback(42, It.IsAny<FuncWrapper>()), Times.Once());
            generatedFuncWrapper(42);
            Assert.AreEqual(protocol.sentMessages[1], "[\"call\",0,\"99095a90-1997-11e3-8ffd-0800200c9a66\",[],42]");
        }

        [Test()]
        public void shouldGenerateDynamicDelegatesForCallbacks()
        {
            protocol.registerHandler("testCallback2", (Action<string,Action<string>>)mockHandlers.Object.testCallback2);
            Action<string> generatedDelegate = null;
            mockHandlers.Setup(h => h.testCallback2("foobar", It.IsAny<Action<string>>()))
                .Callback((string s, Action<string> f) => generatedDelegate = f);
            protocol.handleMessage("['call',0,'testCallback2',[1],'foobar','28abd5c5-14a8-4b4d-8569-7d009bc37f31']");
            mockHandlers.Verify(h => h.testCallback2("foobar", It.IsAny<Action<string>>()), Times.Once());
            generatedDelegate("barfoo");
            Assert.AreEqual("[\"call\",0,\"28abd5c5-14a8-4b4d-8569-7d009bc37f31\",[],\"barfoo\"]",
                            protocol.sentMessages[1]);
        }

        [Test()]
        public void shouldCorrectlyHandleRemoteCallRequestForRegisteredFunctionName()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            protocol.handleMessage("['call',0,'testFunc',[],42,'test-string']");
            mockHandlers.Verify(h => h.testFunc(42, "test-string"), Times.Once());
        }

        [Test()]
        public void shouldFailOnRemoteCallRequestForUnregisteredFunctionName()
        {
            Assert.Throws<UnregisteredMethod>(() => protocol.handleMessage("['call',0,'unknownFunc',[]]"));
        }

//        [Test()]
//        public void shouldReturnErrorToCallerOnInvalidNumberOfArgs()
//        {
//            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
//            protocol.handleMessage("['call',0,'testFunc',[],42]");
//            Assert.AreEqual(protocol.sentMessages[0], "... error ...");
//        }

        [Test()]
        public void shouldFailOnCallReplyWithUnknownCallID()
        {
            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
            Assert.Throws<UnknownCallID>(() => protocol.handleMessage("['call-reply',100,'testFunc',[],42,'foobar']"));
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

