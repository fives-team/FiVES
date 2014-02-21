using System;
using System.ServiceModel;

namespace TestingPlugin
{
    /// <summary>
    /// Testing service used to communicate from the FiVES server to the testing process, which has started it.
    /// </summary>
    [ServiceContract()]
    public interface ITestingService
    {
        /// <summary>
        /// Called when the server is ready, which happens when all plugins are loaded.
        /// </summary>
        [OperationContract]
        void NotifyServerReady();
    }
}
