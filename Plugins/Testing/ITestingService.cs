using System;
using System.ServiceModel;

namespace TestingPlugin
{
    [ServiceContract()]
    public interface ITestingService
    {
        [OperationContract]
        void NotifyServerReady();
    }
}
