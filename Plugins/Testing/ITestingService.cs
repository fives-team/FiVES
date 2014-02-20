using System;
using System.ServiceModel;

namespace TestingPlugin
{
    [ServiceContract(Namespace = "http://sergiyb.com")]
    public interface ITestingService
    {
        [OperationContract]
        void NotifyServerStarted();
    }
}
