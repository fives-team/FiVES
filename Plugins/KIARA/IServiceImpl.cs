using System;
using System.Collections.Generic;

namespace KIARAPlugin
{
    public delegate void NewClient(Connection connection);

    public interface IServiceImpl : IService
    {
        event NewClient OnNewClient;
    }
}

