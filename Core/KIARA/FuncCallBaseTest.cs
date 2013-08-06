using System;
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System.Threading;

namespace KIARA
{
    [TestFixture()]
    public class FuncCallBaseTest
    {
        private class MockFuncCall : FuncCallBase
        {
            #region implemented abstract members of FuncCallBase

            protected override object convertResult(object result, Type type)
            {
                return result;
            }

            #endregion
        }

        public interface IHandler {
            void success(float value);
            void exception(Exception exception);
            void error(string reason);
        }

        private MockFuncCall call;
        private float value = 3.14f;
        private Exception exception = new Exception("test exception");
        private string reason = "test reason";
        private Mock<IHandler> handler;

        [SetUp()]
        public void init()
        {
            call = new MockFuncCall();
            handler = new Mock<IHandler>();
        }

        [Test()]
        public void shouldExecuteSuccessHandlers()
        {
            call.onSuccess((Action<float>)handler.Object.success).onSuccess((Action<float>)handler.Object.success);
            call.handleSuccess(value);
            handler.Verify(h => h.success(value), Times.Exactly(2));
        }

        [Test()]
        public void shouldExecuteExceptionHandlers()
        {
            call.onException(handler.Object.exception).onException(handler.Object.exception);
            call.handleException(exception);
            handler.Verify(h => h.exception(exception), Times.Exactly(2));
        }

        [Test()]
        public void shouldExecuteErrorHandlers()
        {
            call.onError(handler.Object.error).onError(handler.Object.error);
            call.handleError(reason);
            handler.Verify(h => h.error(reason), Times.Exactly(2));
        }

        [Test()]
        public void shouldExecuteSuccessHandlerAddedAfterCallWasCompleted()
        {
            call.handleSuccess(value);
            call.onSuccess((Action<float>)handler.Object.success);
            handler.Verify(h => h.success(value), Times.Once());
        }

        [Test()]
        public void shouldExecuteExceptionHandlerAddedAfterCallWasCompleted()
        {
            call.handleException(exception);
            call.onException(handler.Object.exception);
            handler.Verify(h => h.exception(exception), Times.Once());
        }

        [Test()]
        public void shouldExecuteErrorHandlerAddedAfterCallWasCompleted()
        {
            call.handleError(reason);
            call.onError(handler.Object.error);
            handler.Verify(h => h.error(reason), Times.Once());
        }

        [Test()]
        public void shouldPassReturnValueFromWait()
        {
            call.handleSuccess(value);
            Assert.AreEqual(call.wait<float>(), value);
        }

        [Test()]
        public void shouldRaiseExceptionFromWait()
        {
            call.handleException(exception);
            var thrownException = Assert.Throws<Exception>(() => call.wait());
            Assert.AreEqual(thrownException, exception);
        }

        [Test()]
        public void shouldRaiseErrorFromWait()
        {
            call.handleError(reason);
            Error thrownException = Assert.Throws<Error>(() => call.wait());
            Assert.True(thrownException.Reason.Contains(reason));
        }

        [Test()]
        public void shouldWaitForCompletionOnOtherThread()
        {
            var haveBeenInsideOtherThread = false;
            Task.Factory.StartNew(delegate(object c) {
                Thread.Sleep(100);
                haveBeenInsideOtherThread = true;
                ((FuncCallBase)c).handleSuccess(value);
            }, call);
            Assert.AreEqual(call.wait<float>(), value);
            Assert.True(haveBeenInsideOtherThread);
        }

        [Test()]
        public void shouldTimeoutOnWait()
        {
            Task.Factory.StartNew(delegate(object c) {
                Thread.Sleep(200);
                ((FuncCallBase)c).handleSuccess(value);
            }, call);
            Assert.Throws<TimeoutException>(() => call.wait(100));
        }
    }
}

