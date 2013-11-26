using NUnit.Framework;
using System;
using Moq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using KIARAPlugin;

namespace WebSocketJSON
{
    [TestFixture()]
    public class WSJConnectionTest
    {
        private class WSJConnectionWrapper : WSJConnection
        {
            internal override void Send(string message)
            {
                sentMessages.Add(message);
            }

            public new IFuncCall CallFunc(string funcName, params object[] args)
            {
                return base.CallFunc(funcName, args);
            }

            public void HandleMessage(string message)
            {
                base.HandleMessage(this, new WebSocket4Net.MessageReceivedEventArgs(message));
            }

            public new void RegisterHandler(string funcName, Delegate handler)
            {
                base.RegisterHandler(funcName, handler);
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

        WSJConnectionWrapper connection;
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

            connection = new WSJConnectionWrapper();
            connection.wsjFuncCallFactory = mockWSJFuncCallFactory.Object;
        }

        [Test()]
        public void ShouldCorrectlyFormatCallMessage()
        {
            connection.CallFunc("testFunc", 42, "test-string");
            Assert.AreEqual(connection.sentMessages[0], "[\"call\",0,\"testFunc\",[],42,\"test-string\"]");
        }

        [Test()]
        public void ShouldCorrectlyEncodeNativeCallbacksToTheMessage()
        {
            connection.CallFunc("testFunc", 42, "test-string", (Action)delegate() {});
            Assert.That(connection.sentMessages[0],
                Is.StringMatching("\\[\"call\",0,\"testFunc\",\\[2\\],42,\"test-string\",\"[0-9a-feA-F\\-]+\"\\]"));
        }

        [Test()]
        public void ShouldSendCallReplies()
        {
            connection.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            mockHandlers.Setup(h => h.TestFunc(42, "test-string")).Returns(3.14f);
            connection.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            Assert.AreEqual(connection.sentMessages[0], "[\"call-reply\",0,true,3.14]");
        }

        [Test()]
        public void ShouldHandleSuccessCallReply()
        {
            connection.CallFunc("testFunc", 42, "test-string");
            connection.HandleMessage("['call-reply',0,true,3.14]");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void ShouldHandleExceptionCallReply()
        {
            connection.CallFunc("testFunc", 42, "test-string");
            connection.HandleMessage("['call-reply',0,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.HandleException(It.IsAny<JToken>()), Times.Once());
        }

        [Test()]
        public void ShouldFailAllActiveCallsWithErrorOnClose()
        {
            connection.CallFunc("testFunc1", 42, "test-string");
            connection.CallFunc("testFunc2", "foobar", 123);
            connection.HandleClosed(this, new EventArgs());

            mockWSJFuncCall.Verify(c => c.HandleError(It.IsAny<string>()), Times.Exactly(2));
        }

        [Test()]
        public void ShouldProcessConcurrentCallsCorrectly()
        {
            connection.CallFunc("testFunc1", 42, "test-string");
            connection.CallFunc("testFunc2", "foobar", 123);
            connection.HandleMessage("['call-reply',0,true,'ret-val-1']");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Once());
            connection.CallFunc("testFunc3");
            connection.HandleMessage("['call-reply',2,false,'oops!']");
            mockWSJFuncCall.Verify(c => c.HandleException(It.IsAny<JToken>()), Times.Once());
            connection.HandleMessage("['call-reply',1,true,'ret-val-2']");
            mockWSJFuncCall.Verify(c => c.HandleSuccess(It.IsAny<JToken>()), Times.Exactly(2));
        }

        [Test()]
        public void ShouldProcessCallReplyWithNoRetValueCorrectly()
        {
            connection.CallFunc("testFunc1", 42, "test-string");
            connection.HandleMessage("['call-reply',0,true]");
        }

        [Test()]
        public void ShouldHandleRemoteCallbacksCorrectly()
        {
            connection.RegisterHandler("testCallback", (Action<int, FuncWrapper>)mockHandlers.Object.TestCallback);
            FuncWrapper generatedFuncWrapper = null;
            mockHandlers.Setup(h => h.TestCallback(42, It.IsAny<FuncWrapper>()))
                .Callback((int i, FuncWrapper f) => generatedFuncWrapper = f);
            connection.HandleMessage("['call',0,'testCallback',[1],42,'99095a90-1997-11e3-8ffd-0800200c9a66']");
            mockHandlers.Verify(h => h.TestCallback(42, It.IsAny<FuncWrapper>()), Times.Once());
            generatedFuncWrapper(42);
            Assert.AreEqual(connection.sentMessages[1], "[\"call\",0,\"99095a90-1997-11e3-8ffd-0800200c9a66\",[],42]");
        }

        [Test()]
        public void ShouldGenerateDynamicDelegatesForCallbacks()
        {
            connection.RegisterHandler("testCallback2", (Action<string,Action<string>>)mockHandlers.Object.TestCallback2);
            Action<string> generatedDelegate = null;
            mockHandlers.Setup(h => h.TestCallback2("foobar", It.IsAny<Action<string>>()))
                .Callback((string s, Action<string> f) => generatedDelegate = f);
            connection.HandleMessage("['call',0,'testCallback2',[1],'foobar','28abd5c5-14a8-4b4d-8569-7d009bc37f31']");
            mockHandlers.Verify(h => h.TestCallback2("foobar", It.IsAny<Action<string>>()), Times.Once());
            generatedDelegate("barfoo");
            Assert.AreEqual("[\"call\",0,\"28abd5c5-14a8-4b4d-8569-7d009bc37f31\",[],\"barfoo\"]",
                            connection.sentMessages[1]);
        }

        [Test()]
        public void ShouldCorrectlyHandleRemoteCallRequestForRegisteredFunctionName()
        {
            connection.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            connection.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            mockHandlers.Verify(h => h.TestFunc(42, "test-string"), Times.Once());
        }

        [Test()]
        public void ShouldSendErrorFeedbackOnRemoteCallRequestForUnregisteredFunctionName()
        {
            connection.HandleMessage("['call',0,'unknownFunc',[]]");
            Assert.AreEqual(connection.sentMessages[0], "[\"call-error\",0,\"Method unknownFunc is not registered\"]");
        }

        [Test()]
        public void ShouldReturnErrorToCallerOnInvalidNumberOfArgs()
        {
            connection.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            connection.HandleMessage("['call',0,'testFunc',[],42]");
            Assert.AreEqual("[\"call-error\",0,\"Incorrect number of arguments for a method. Expected: 2. " +
                "Received: 1\"]", connection.sentMessages[0]);
        }

        [Test()]
        public void ShouldSendErrorFeedbackOnCallReplyWithUnknownCallID()
        {
            connection.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            connection.HandleMessage("['call-reply',100,'testFunc',[],42,'foobar']");
            Assert.AreEqual(connection.sentMessages[0], "[\"call-error\",-1,\"Invalid callID: 100\"]");
        }

        [Test()]
        public void ShouldReregisterHandlerForTheSameFunctionName()
        {
            connection.RegisterHandler("testFunc", (Func<int, string, float>)mockHandlers.Object.TestFunc);
            connection.RegisterHandler("testFunc", (Action<float>)mockHandlers.Object.TestFunc2);
            connection.HandleMessage("['call',0,'testFunc',[],3.14]");
            mockHandlers.Verify(h => h.TestFunc(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            mockHandlers.Verify(h => h.TestFunc2(3.14f), Times.Once());
        }

        [Test()]
        public void ShouldCorrectlySupplyConnectionParameter()
        {
            connection.RegisterHandler("testFunc",
                                       (Action<Connection, int, string>)mockHandlers.Object.TestFuncWithConn);
            connection.HandleMessage("['call',0,'testFunc',[],42,'test-string']");
            mockHandlers.Verify(h => h.TestFuncWithConn(connection, 42, "test-string"), Times.Once());
        }

        [Test()]
        public void ShouldCorrectlyHandleFloatAndDoubleNaNAndInfinity()
        {
            connection.CallFunc("testFunc", float.NaN, float.PositiveInfinity, float.NegativeInfinity, double.NaN,
                                double.PositiveInfinity, double.NegativeInfinity);
            Assert.AreEqual("[\"call\",0,\"testFunc\",[],null,null,null,null,null,null]", connection.sentMessages[0]);
        }

        // TODO: Should process IDL (when implemented).
    }
}

