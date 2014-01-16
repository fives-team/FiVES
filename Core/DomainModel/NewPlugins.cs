using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIVES_NewPlugins
{
    #region KIARA Plugin

    public interface IFuncCall
    {
        IFuncCall OnSuccess<T>(Action<T> handler);
        IFuncCall OnSuccess(Action handler);
        IFuncCall OnException(Action<Exception> handler);
        IFuncCall OnError(Action<string> handler);

        // Perhaps also:
        //   OnCompleted = OnSuccess + OnException + OnError
        //   OnFailure = OnException + OnError
        //   OnResult = OnSuccess + OnException

        // Maybe add events:
        //   event EventHandler<SuccessEventArgs> Succeeded;
        //   event EventHandler<ExceptionEventArgs> ThrownException;
        //   event EventHandler<ErrorEventArgs> CausedError;
        //   event EventHandler Completed;
        //   event EventHandler Failed;
        //   event EventHandler ReturnedResult;

        T Wait<T>(int millisecondsTimeout = -1);
        void Wait(int millisecondsTimeout = -1);
    }

    public delegate IFuncCall FuncWrapper(params object[] args);

    public interface IConnection
    {
        FuncWrapper this[string name] { get; }

        void LoadIDL(string uri);

        void RegisterFuncImplementation(string funcName, Delegate handler, string typeMapping = "");
        FuncWrapper GenerateFuncWrapper(string funcName, string typeMapping = "");

        bool GetProperty(string name, out object value);
        bool SetProperty(string name, object value);

        void Disconnect();

        event EventHandler Closed;
    }

    #endregion

    #region ClientManager Plugin

    public enum ClientStatus
    {
        Connected,
        Authenticated,
        Disconnected
    }

    public interface IClient : IConnection
    {
        ClientStatus Status { get; }
    }

    public class ClientEventArgs : EventArgs
    {
        public ClientEventArgs(IClient client)
        {
            Client = client;
        }

        public IClient Client { get; private set; }
    }

    public interface IClientList
    {
        IEnumerable<IClient> Clients { get; }

        event EventHandler<ClientEventArgs> ConnectedClient;
        event EventHandler<ClientEventArgs> AuthententicatedClient;
        event EventHandler<ClientEventArgs> DisconnectedClient;
    }

    public class ClientDataEventArgs : ClientEventArgs
    {
        public ClientDataEventArgs(IClient client, string key, byte[] oldData, byte[] newData)
            : base(client)
        {
            Key = key;
            OldData = oldData;
            NewData = newData;
        }

        public string Key { get; private set; }
        public byte[] OldData { get; private set; }
        public byte[] NewData { get; private set; }
    }

    public interface IClientData
    {
        void SetClientData(IClient client, string key, byte[] value);
        byte[] GetClientData(IClient client, string key);

        event EventHandler<ClientDataEventArgs> ModifiedData;
    }

    public interface IClientAPI
    {
        void RegisterClientMethod(string methodName, bool requireAuthentication, Delegate handler);
        void RegisterClientService(string serviceName, bool requireAuthentication,
            Dictionary<string, Delegate> methods);
    }

    public static class ClientManager
    {
        public static IClientList List;
        public static IClientData Data;
        public static IClientAPI API;
    }

    #endregion

    #region Authentication Plugin

    public interface IAuthentication
    {
        void Authenticate(IClient client, Action<IClient, bool> callback);
    }

    public static class Authentication
    {
        public static IAuthentication Instance;
    }

    #endregion
}
