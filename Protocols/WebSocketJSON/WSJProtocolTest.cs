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
            float TestFunc(int i, string s);
            void TestFunc2(float x);
            void TestCallback(int i, FuncWrapper callback);
            void TestCallback2(string a, Action<string> hello);
        }

        WSJProtocolWrapper protocol;
        Mock<IWSJFuncCall> mockWSJFuncCall;
        Mock<IWSJFuncCallFactory> mockWSJFuncCallFactory;
        Mock<IHandlers> mockHandlers;

        [SetUp()]
        public void Init()
        {
            mockWSJFuncCallFactory = new Mock<IWSJFuncCallFactory>();
            mockWSJFuncCall = new Mock<IWSJFuncCall>();
            mockWSJFuncCallFactory.Setup(f => f.Construct()).Returns(mockWSJFuncCall.Object);
            mockHandlers = new Mock<IHandlers>();

            protocol = new WSJProtocolWrapper(mockWSJFuncCallFactory.Object);
        }

        [Test()]
        public void ShouldCorrectlyFormatCallMessage()
        {
            protocol.CallFunc("testFunc", 42, "test-string");
            Assert.AreEqual(protocol.sentMessages[0], "[\"call\",0,\"testFunc\",[],42,\"test-string\"]");
        }

        [Test()]
        public void ShouldCorrectlyEncodeNativeCallbacksToTheMessage()
        {
            protocol.CallFunc("testFunc", 42, "test-string", (Action)delegate() {});
            Assert.That(protocol.sentMessages[0],
                Is.StringMatching("\\[\"call\",0,\"testFunc\",\\[2\\],42,\"test-string\",\"[0-9a-feA-F\\-]+\"\\]"));
        }

        [Test()]
        public void ShouldSendCallReplies()
        {
            protocol.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            mockHandlers.Setup(h => h.TestFunc(42, "test-string")).Returns(3.14f);
            protocol.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            Assert.AreEqual(protocol.sentMessages[0], "[\"call-reply\",0,true,3.14]");
        }

        [Test()]
        public void ShouldHandleSuccessCallReply()
        {
            protocol.CallFunc("testFunc", 42, "test-string");
            protocol.HandleMessage("['call-reply',0,true,3.14]");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void ShouldHandleExceptionCallReply()
        {
            protocol.CallFunc("testFunc", 42, "test-string");
            protocol.HandleMessage("['call-reply',0,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.HandleException(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void ShouldFailAllActiveCallsWithErrorOnClose()
        {
            protocol.CallFunc("testFunc1", 42, "test-string");
            protocol.CallFunc("testFunc2", "foobar", 123);
            protocol.HandleClose(SuperSocket.SocketBase.CloseReason.InternalError);

            mockWSJFuncCall.Verify(c => c.HandleError(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test()]
        public void ShouldProcessConcurrentCallsCorrectly()
        {
            protocol.CallFunc("testFunc1", 42, "test-string");
            protocol.CallFunc("testFunc2", "foobar", 123);
            protocol.HandleMessage("['call-reply',0,true,'ret-val-1']");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Once());
            protocol.CallFunc("testFunc3");
            protocol.HandleMessage("['call-reply',2,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.HandleException(It.IsAny<JToken>()), Times.Once());
            protocol.HandleMessage("['call-reply',1,true,'ret-val-2']");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Exactly(2));
        }

        [Test()]
        public void ShouldProcessCallReplyWithNoRetValueCorrectly()
        {
            protocol.CallFunc("testFunc1", 42, "test-string");
            protocol.HandleMessage("['call-reply',0,true]");
        }

        [Test()]
        public void ShouldHandleRemoteCallbacksCorrectly()
        {
            protocol.RegisterHandler("testCallback", (Action<int, FuncWrapper>)mockHandlers.Object.TestCallback);
            FuncWrapper generatedFuncWrapper = null;
            mockHandlers.Setup(h => h.TestCallback(42, It.IsAny<FuncWrapper>()))
                .Callback((int i, FuncWrapper f) => generatedFuncWrapper = f);
            protocol.HandleMessage("['call',0,'testCallback',[1],42,'99095a90-1997-11e3-8ffd-0800200c9a66']");
            mockHandlers.Verify(h => h.TestCallback(42, It.IsAny<FuncWrapper>()), Times.Once());
            generatedFuncWrapper(42);
            Assert.AreEqual(protocol.sentMessages[1], "[\"call\",0,\"99095a90-1997-11e3-8ffd-0800200c9a66\",[],42]");
        }

        [Test()]
        public void ShouldGenerateDynamicDelegatesForCallbacks()
        {
            protocol.RegisterHandler("testCallback2", (Action<string,Action<string>>)mockHandlers.Object.TestCallback2);
            Action<string> generatedDelegate = null;
            mockHandlers.Setup(h => h.TestCallback2("foobar", It.IsAny<Action<string>>()))
                .Callback((string s, Action<string> f) => generatedDelegate = f);
            protocol.HandleMessage("['call',0,'testCallback2',[1],'foobar','28abd5c5-14a8-4b4d-8569-7d009bc37f31']");
            mockHandlers.Verify(h => h.TestCallback2("foobar", It.IsAny<Action<string>>()), Times.Once());
            generatedDelegate("barfoo");
            Assert.AreEqual("[\"call\",0,\"28abd5c5-14a8-4b4d-8569-7d009bc37f31\",[],\"barfoo\"]",
                            protocol.sentMessages[1]);
        }

        [Test()]
        public void ShouldCorrectlyHandleRemoteCallRequestForRegisteredFunctionName()
        {
            protocol.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            protocol.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            mockHandlers.Verify(h => h.TestFunc(42, "test-string"), Times.Once());
        }

        [Test()]
        public void ShouldFailOnRemoteCallRequestForUnregisteredFunctionName()
        {
            Assert.Throws<UnregisteredMethod>(() => protocol.HandleMessage("['call',0,'unknownFunc',[]]"));
        }

//        [Test()]
//        public void ShouldReturnErrorToCallerOnInvalidNumberOfArgs()
//        {
//            protocol.registerHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.testFunc);
//            protocol.handleMessage("['call',0,'testFunc',[],42]");
//            Assert.AreEqual(protocol.sentMessages[0], "... error ...");
//        }

        [Test()]
        public void ShouldFailOnCallReplyWithUnknownCallID()
        {
            protocol.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            Assert.Throws<UnknownCallID>(() => protocol.HandleMessage("['call-reply',100,'testFunc',[],42,'foobar']"));
        }

        [Test()]
        public void ShouldReregisterHandlerForTheSameFunctionName()
        {
            protocol.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            protocol.RegisterHandler("testFunc", (Action<float>)mockHandlers.Object.TestFunc2);
            protocol.HandleMessage("['call',0,'testFunc',[],3.14]");
            mockHandlers.Verify(h => h.TestFunc(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            mockHandlers.Verify(h => h.TestFunc2(3.14f), Times.Once());
        }

        // TODO: Should process IDL (when implemented).
    }
}
