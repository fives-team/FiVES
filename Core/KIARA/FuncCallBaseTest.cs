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

            protected override object ConvertResult(object result, Type type)
            {
                return result;
            }

            #endregion
        }

        public interface IHandler {
            void Success(float value);
            void Exception(Exception exception);
            void Error(string reason);
        }

        private MockFuncCall call;
        private float value = 3.14f;
        private Exception exception = new Exception("test exception");
        private string reason = "test reason";
        private Mock<IHandler> handler;

        [SetUp()]
        public void Init()
        {
            call = new MockFuncCall();
            handler = new Mock<IHandler>();
        }

        [Test()]
        public void ShouldExecuteSuccessHandlers()
        {
            call.OnSuccess((Action<float>)handler.Object.Success).OnSuccess((Action<float>)handler.Object.Success);
            call.HandleSuccess(value);
            handler.Verify(h => h.Success(value), Times.Exactly(2));
        }

        [Test()]
        public void ShouldExecuteExceptionHandlers()
        {
            call.OnException(handler.Object.Exception).OnException(handler.Object.Exception);
            call.HandleException(exception);
            handler.Verify(h => h.Exception(exception), Times.Exactly(2));
        }

        [Test()]
        public void ShouldExecuteErrorHandlers()
        {
            call.OnError(handler.Object.Error).OnError(handler.Object.Error);
            call.HandleError(reason);
            handler.Verify(h => h.Error(reason), Times.Exactly(2));
        }

        [Test()]
        public void ShouldExecuteSuccessHandlerAddedAfterCallWasCompleted()
        {
            call.HandleSuccess(value);
            call.OnSuccess((Action<float>)handler.Object.Success);
            handler.Verify(h => h.Success(value), Times.Once());
        }

        [Test()]
        public void ShouldExecuteExceptionHandlerAddedAfterCallWasCompleted()
        {
            call.HandleException(exception);
            call.OnException(handler.Object.Exception);
            handler.Verify(h => h.Exception(exception), Times.Once());
        }

        [Test()]
        public void ShouldExecuteErrorHandlerAddedAfterCallWasCompleted()
        {
            call.HandleError(reason);
            call.OnError(handler.Object.Error);
            handler.Verify(h => h.Error(reason), Times.Once());
        }

        [Test()]
        public void ShouldPassReturnValueFromWait()
        {
            call.HandleSuccess(value);
            Assert.AreEqual(call.Wait<float>(), value);
        }

        [Test()]
        public void ShouldRaiseExceptionFromWait()
        {
            call.HandleException(exception);
            var thrownException = Assert.Throws<Exception>(() => call.Wait());
            Assert.AreEqual(thrownException, exception);
        }

        [Test()]
        public void ShouldRaiseErrorFromWait()
        {
            call.HandleError(reason);
            Error thrownException = Assert.Throws<Error>(() => call.Wait());
            Assert.True(thrownException.Reason.Contains(reason));
        }

        [Test()]
        public void ShouldWaitForCompletionOnOtherThread()
        {
            var haveBeenInsideOtherThread = false;
            Task.Factory.StartNew(delegate(object c) {
                Thread.Sleep(100);
                haveBeenInsideOtherThread = true;
                ((FuncCallBase)c).HandleSuccess(value);
            }, call);
            Assert.AreEqual(call.Wait<float>(), value);
            Assert.True(haveBeenInsideOtherThread);
        }

        [Test()]
        public void ShouldTimeoutOnWait()
        {
            Task.Factory.StartNew(delegate(object c) {
                Thread.Sleep(200);
                ((FuncCallBase)c).HandleSuccess(value);
            }, call);
            Assert.Throws<TimeoutException>(() => call.Wait(100));
        }
    }
}

