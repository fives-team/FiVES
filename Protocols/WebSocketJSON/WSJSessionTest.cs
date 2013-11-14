using NUnit.Framework;
using System;
using Moq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using KIARAPlugin;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJSessionTest
    {
        private class WSJSessionWrapper : WSJSession
        {
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
            void TestFuncWithConn(Connection conn, int i, string s);
        }

        WSJSessionWrapper session;
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

            session = new WSJSessionWrapper();
            session.wsjFuncCallFactory = mockWSJFuncCallFactory.Object;
        }

        [Test()]
        public void ShouldCorrectlyFormatCallMessage()
        {
            session.CallFunc("testFunc", 42, "test-string");
            Assert.AreEqual(session.sentMessages[0], "[\"call\",0,\"testFunc\",[],42,\"test-string\"]");
        }

        [Test()]
        public void ShouldCorrectlyEncodeNativeCallbacksToTheMessage()
        {
            session.CallFunc("testFunc", 42, "test-string", (Action)delegate() {});
            Assert.That(session.sentMessages[0],
                Is.StringMatching("\\[\"call\",0,\"testFunc\",\\[2\\],42,\"test-string\",\"[0-9a-feA-F\\-]+\"\\]"));
        }

        [Test()]
        public void ShouldSendCallReplies()
        {
            session.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            mockHandlers.Setup(h => h.TestFunc(42, "test-string")).Returns(3.14f);
            session.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            Assert.AreEqual(session.sentMessages[0], "[\"call-reply\",0,true,3.14]");
        }

        [Test()]
        public void ShouldHandleSuccessCallReply()
        {
            session.CallFunc("testFunc", 42, "test-string");
            session.HandleMessage("['call-reply',0,true,3.14]");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void ShouldHandleExceptionCallReply()
        {
            session.CallFunc("testFunc", 42, "test-string");
            session.HandleMessage("['call-reply',0,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.HandleException(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void ShouldFailAllActiveCallsWithErrorOnClose()
        {
            session.CallFunc("testFunc1", 42, "test-string");
            session.CallFunc("testFunc2", "foobar", 123);
            session.HandleClose(SuperSocket.SocketBase.CloseReason.InternalError);

            mockWSJFuncCall.Verify(c => c.HandleError(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test()]
        public void ShouldProcessConcurrentCallsCorrectly()
        {
            session.CallFunc("testFunc1", 42, "test-string");
            session.CallFunc("testFunc2", "foobar", 123);
            session.HandleMessage("['call-reply',0,true,'ret-val-1']");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Once());
            session.CallFunc("testFunc3");
            session.HandleMessage("['call-reply',2,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.HandleException(It.IsAny<JToken>()), Times.Once());
            session.HandleMessage("['call-reply',1,true,'ret-val-2']");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Exactly(2));
        }

        [Test()]
        public void ShouldProcessCallReplyWithNoRetValueCorrectly()
        {
            session.CallFunc("testFunc1", 42, "test-string");
            session.HandleMessage("['call-reply',0,true]");
        }

        [Test()]
        public void ShouldHandleRemoteCallbacksCorrectly()
        {
            session.RegisterHandler("testCallback", (Action<int, FuncWrapper>)mockHandlers.Object.TestCallback);
            FuncWrapper generatedFuncWrapper = null;
            mockHandlers.Setup(h => h.TestCallback(42, It.IsAny<FuncWrapper>()))
                .Callback((int i, FuncWrapper f) => generatedFuncWrapper = f);
            session.HandleMessage("['call',0,'testCallback',[1],42,'99095a90-1997-11e3-8ffd-0800200c9a66']");
            mockHandlers.Verify(h => h.TestCallback(42, It.IsAny<FuncWrapper>()), Times.Once());
            generatedFuncWrapper(42);
            Assert.AreEqual(session.sentMessages[1], "[\"call\",0,\"99095a90-1997-11e3-8ffd-0800200c9a66\",[],42]");
        }

        [Test()]
        public void ShouldGenerateDynamicDelegatesForCallbacks()
        {
            session.RegisterHandler("testCallback2", (Action<string,Action<string>>)mockHandlers.Object.TestCallback2);
            Action<string> generatedDelegate = null;
            mockHandlers.Setup(h => h.TestCallback2("foobar", It.IsAny<Action<string>>()))
                .Callback((string s, Action<string> f) => generatedDelegate = f);
            session.HandleMessage("['call',0,'testCallback2',[1],'foobar','28abd5c5-14a8-4b4d-8569-7d009bc37f31']");
            mockHandlers.Verify(h => h.TestCallback2("foobar", It.IsAny<Action<string>>()), Times.Once());
            generatedDelegate("barfoo");
            Assert.AreEqual("[\"call\",0,\"28abd5c5-14a8-4b4d-8569-7d009bc37f31\",[],\"barfoo\"]",
                            session.sentMessages[1]);
        }

        [Test()]
        public void ShouldCorrectlyHandleRemoteCallRequestForRegisteredFunctionName()
        {
            session.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            session.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            mockHandlers.Verify(h => h.TestFunc(42, "test-string"), Times.Once());
        }

        [Test()]
        public void ShouldSendErrorFeedbackOnRemoteCallRequestForUnregisteredFunctionName()
        {
            session.HandleMessage("['call',0,'unknownFunc',[]]");
            Assert.AreEqual(session.sentMessages[0], "[\"call-error\",0,\"Method unknownFunc is not registered\"]");
        }

        [Test()]
        public void ShouldReturnErrorToCallerOnInvalidNumberOfArgs()
        {
            session.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            session.HandleMessage("['call',0,'testFunc',[],42]");
            Assert.AreEqual("[\"call-error\",0,\"Incorrect number of arguments for a method. Expected: 2. " +
                "Received: 1\"]", session.sentMessages[0]);
        }

        [Test()]
        public void ShouldSendErrorFeedbackOnCallReplyWithUnknownCallID()
        {
            session.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            session.HandleMessage("['call-reply',100,'testFunc',[],42,'foobar']");
            Assert.AreEqual(session.sentMessages[0], "[\"call-error\",-1,\"Invalid callID: 100\"]");
        }

        [Test()]
        public void ShouldReregisterHandlerForTheSameFunctionName()
        {
            session.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            session.RegisterHandler("testFunc", (Action<float>)mockHandlers.Object.TestFunc2);
            session.HandleMessage("['call',0,'testFunc',[],3.14]");
            mockHandlers.Verify(h => h.TestFunc(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            mockHandlers.Verify(h => h.TestFunc2(3.14f), Times.Once());
        }

        [Test()]
        public void ShouldCorrectlySupplyConnectionParameter()
        {
            session.RegisterHandler("testFunc", (Action<Connection, int, string>)mockHandlers.Object.TestFuncWithConn);
            session.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            mockHandlers.Verify(h => h.TestFuncWithConn(session.connectionAdapter, 42, "test-string"), Times.Once());
        }

        // TODO: Should process IDL (when implemented).
    }
}

