using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FIVES_New
{
    #region KIARA (incomplete)

    // Plug-in depedencies: none.
    // Component dependencies: none.

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

    public interface IContext
    {
        void Initialize(string hint);
        void OpenConnection(string configURI, Action<IConnection> onConnected);
        void StartServer(string configURI, Action<IConnection> onNewClient);
    }

    public interface IService
    {
        IContext context { get; }
    }

    public delegate void NewClient(IConnection connection);

    public interface ILocalService : IService
    {
        event NewClient OnNewClient;
        Delegate this[string name] { set; }
    }

    public delegate void Connected(IConnection connection);

    public interface IRemoteService : IService
    {
        event Connected OnConnected;
    }

    public interface IKIARA
    {
        ILocalService Create(string configURI);
        IRemoteService Discover(string configURI);
    }

    public static class KIARA
    {
        public static IKIARA Instance;
        public static IContext DefaultContext;

        public static ILocalService Create(string configURI)
        {
            return Instance.Create(configURI);
        }

        public static IRemoteService Discover(string configURI)
        {
            return Instance.Discover(configURI);
        }
    }

    #endregion

    #region ClientManager

    // Plug-in depedencies: KIARA, Authentication.
    // Component dependencies: none.

    // An interface implemented by the client object.
    public interface IAuthenticatedClient
    {
        // Connection to the client. Interface declared in KIARA.
        IConnection Connection { get; }

        // User account for the client. Interface declared in Authentication.
        IUserAccount UserAccount { get; }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(IConnection connection)
        {
            Connection = connection;
        }

        public IConnection Connection { get; private set; }
    }

    public class ClientAuthenticatedEventArgs : EventArgs
    {
        public ClientAuthenticatedEventArgs(IAuthenticatedClient client)
        {
            Client = client;
        }

        public IAuthenticatedClient Client { get; private set; }
    }

    // An interface to keep track of connected clients and their state.
    public interface IClientList
    {
        // Keeps a list of the authenticated clients.
        IEnumerable<IAuthenticatedClient> AuthenticatedClients { get; }

        // Events to keep track of client status changes.
        event EventHandler<ConnectionEventArgs> Connected;
        event EventHandler<ClientAuthenticatedEventArgs> Authententicated;
        event EventHandler<ConnectionEventArgs> Disconnected;
    }

    // An interface to manage functions exposed to the client.
    public interface IClientAPI
    {
        // Registers a named service with a set of methods. Clients invoke functions as "serviceName.methodName". When
        // last parameter is set to true, the service is only available to the authenticated clients.
        void RegisterService(string serviceName, IDictionary<string, Delegate> methods,
            bool requireAuthentication = true);
    }

    public static class ClientManager
    {
        public static IClientList List;
        public static IClientAPI API;
    }

    #endregion

    #region Authentication

    // Plug-in depedencies: KIARA.
    // Component dependencies: none.

    // Interface of an object that represents a user account.
    public interface IUserAccount
    {
        // Returns unique user account id.
        string GetId();
    }

    // Callback used to report authentication results. If the authentication has failed, the authenticatedUser will be set
    // to null. The clientConnection parameter is the same which was passed to the Authenticate method.
    public delegate void AuthenticationCallback(IConnection clientConnection, IUserAccount userAccount);

    // An interface to authenticate connected clients.
    public interface IAuthentication
    {
        // Method which starts authentication process. When the authentication is complete, the callback is invoked.
        void Authenticate(IConnection clientConnection, AuthenticationCallback callback);

        // May be used to get the user account object from the user account id.
        IUserAccount GetUserAccount(string userAccountId);
    }

    public static class Authentication
    {
        public static IAuthentication Instance;
    }

    #endregion

    #region ClientSync

    // Plug-in depedencies: ClientManager.
    // Component dependencies: none.

    public interface IUpdate
    {
        int Id { get; }
        IAuthenticatedClient Client { get; }
        IEntity Entity { get; }
        IComponentDefinition Component { get; }
        string AttributeName { get; }
        object NewValue { get; }
        bool RequestACK { get; }
    }

    public interface IChange
    {
        IAuthenticatedClient Client { get; }
        IEntity Entity { get; }
        IComponentDefinition Component { get; }
        string AttributeName { get; }
        object OldValue { get; }
        object NewValue { get; }
        DateTime TimeChanged { get; }
    }

    public interface ISyncPolicy
    {
        // Selects attribute changes, which should be synchronized. Selected changes are sent as updates to the client
        // in the enumeration order until the bandwidth is exhausted. The remaining changes are dropped. The actual
        // number of bytes used by the updates that were sent is passed to the next call as a hint.
        IEnumerable<IUpdate> SelectChangesToSync(IEnumerable<IChange> changes, IEnumerable<int> usedBytesPerChange,
            IEnumerable<int> ackedUpdates);
    }

    public interface IAttributeSyncPolicyCollection
    {
        // Access or set attribute synchronization policy.
        ISyncPolicy this[string attributeName] { get; set; }
    }

    public interface IComponentSyncPolicyCollection
    {
        // Access or set component synchronization policy.
        IAttributeSyncPolicyCollection this[string componentName] { get; }
    }

    public interface IEqualSharePolicy : ISyncPolicy
    {
        // Custom component policies.
        IComponentSyncPolicyCollection ComponentPolicies { get; }

        // Default component policy. By default it is set to LastReliablePolicy.
        ISyncPolicy DefaultComponentPolicy { get; set; }

        // Policy which synchronizes all changes reliably. Dropped changes are retried until successful.
        ISyncPolicy ReliablePolicy { get; }

        // Policy which returns all passed changes and ignores all dropped changes.
        ISyncPolicy UnreliablePolicy { get; }

        // Policy which attempts to re-send dropped changes, but drops them if the same attribute has changed again.
        ISyncPolicy LastReliablePolicy { get; }

        // Policy which drops all changes and returns an empty collection.
        ISyncPolicy UnsynchronizedPolicy { get; }
    }

    public static class ClientSync
    {
        // Policy currently used to synchronize all changes. May be assigned to replace default policy.
        public static ISyncPolicy CurrentPolicy;

        // Default policy used by the Client Sync. Distributes bandwidths equally to all clients, entities and
        // components, but allows to set custom component policies. This may be used by plug-ins which introduce new 
        // components to define most reasonable policy.
        public static IEqualSharePolicy DefaultPolicy;
    }

    #endregion

    #region Avatar

    // Plug-in depedencies: ClientManager.
    // Component dependencies: meshResource, scale, velocity, rotVelocity.

    // No plug-in API.

    #endregion

    #region Animation

    // Plug-in depedencies: ClientManager, EventLoop.
    // Component dependencies: none.

    // Defines animation configuration.
    public interface IAnimationConfig
    {
        float StartFrame { get; }
        float EndFrame { get; }
        int Cycles { get; }
        float Speed { get; }
    }

    public class AnimationEventArgs : EventArgs
    {
        public AnimationEventArgs(IEntity entity, string animationName, IAnimationConfig config)
        {
            Entity = entity;
            AnimationName = animationName;
            Config = config;
        }

        public IEntity Entity { get; private set; }
        public string AnimationName { get; private set; }
        public IAnimationConfig Config { get; private set; }
    }

    public interface IRunningAnimation
    {
        IEntity Entity { get; }
        string AnimationName { get; }
        IAnimationConfig Config { get; }
    }

    public interface IAnimation
    {
        // Methods to control server- and client-side animations.
        void StartServersideAnimation(IEntity entity, string animationName, IAnimationConfig config);
        void StopServersideAnimation(IEntity entity, string animationName);
        void StartClientsideAnimation(IEntity entity, string animationName, IAnimationConfig config);
        void StopClientsideAnimation(IEntity entity, string animationName);

        IEnumerable<IRunningAnimation> RunningClientAnimations { get; }

        // Occur when a client side animations are started or stopped.
        event EventHandler<AnimationEventArgs> StartedClientAnimation;
        event EventHandler<AnimationEventArgs> StoppedClientAnimation;

        // Constructs an animation configuration object.
        IAnimationConfig CreateAnimationConfig(float startFrame, float endFrame, int cycles, float speed);
    }

    public static class Animation
    {
        public static IAnimation Instance;
    }

    #endregion

    #region EventLoop

    // Plug-in depedencies: none.
    // Component dependencies: none.

    // Method invoked to progress a simulation.
    public delegate void ProgressFunc(TimeSpan timeSinceLastTickMs);

    // Represents a running simulation.
    public interface ISimulation
    {
        ProgressFunc ProgressFunc { get; }
        float IntervalMs { get; }
    }

    public interface IEventLoop
    {
        // Starts a simulation running at a specified rate.
        ISimulation StartSimulation(ProgressFunc progressFunc, float intervalMs);

        // Stops a running simulation.
        void StopSimulation(ISimulation runningSimulation);
    }

    public static class EventLoop
    {
        public static IEventLoop Instance;
    }

    #endregion

    #region Motion

    // Plug-in depedencies: EventLoop.
    // Component dependencies: position, orientation.

    // No plug-in API.

    #endregion

    #region Physics

    public interface IVector
    {
        double X { get; }
        double Y { get; }
        double Z { get; }
    }

    // component physics {
    //   
    // }

    public interface IPhysics
    {
        void ApplyImpulse(IEntity entity, IVector force);

        IVector CreateVector(double x, double y, double z);
    }

    public static class Physics
    {
        public static IPhysics Instance;
    }

    #endregion

    #region Authoring (former Editing)

    // Plug-in depedencies: ClientManager.
    // Component dependencies: position, orientation, scale, meshResource.

    // No public API.

    #endregion

    #region Scripting

    // Plug-in depedencies: none.
    // Component dependencies: none.

    // Represent a scripting context.
    public interface IJSContext
    {
        // Executes a script. Computed value is converted to a C# object and returned.
        object Execute(string script);
    }

    public class ScriptEventArgs : EventArgs
    {
        public ScriptEventArgs(object[] eventArgs)
        {
            EventArgs = eventArgs;
        }

        public object[] EventArgs { get; private set; }
    }

    // Interface implemented by the custom event provider.
    public interface IEventProvider
    {
        event EventHandler<ScriptEventArgs> RaisedEvent;
    }

    public class JSContextEventArgs : EventArgs
    {
        public JSContextEventArgs(IJSContext context)
        {
            Context = context;
        }

        public IJSContext Context { get; private set; }
    }

    // Interface to allow modifying scripting environment for the server scripts.
    public interface IScripting
    {
        // Registers a new global object for server scripts.
        void RegisterGlobalObject(string name, object cSharpObject);

        // Registers a new event available for server scripts.
        void RegisterNewEvent(string name, IEventProvider eventProvider);

        // Event is invoked when a new context is created for an entity with server script. Script will already contain
        // all configured global objects. This can be used to perform advanced context initialization.
        event EventHandler<JSContextEventArgs> CreatedContext;
    }

    public static class Scripting
    {
        public static IScripting Instance;
    }
    
    #endregion

    #region Persistence

    // Plug-in depedencies: none.
    // Component dependencies: none.

    public interface ICustomDataCollection
    {
        object this[string key] { get; set; }
    }

    public static class Persistence
    {
        // Collection to access persisted custom plug-in data.
        public static ICustomDataCollection CustomData;
    }

    #endregion

    #region ServerSync

    // Plug-in depedencies: KIARA.
    // Component dependencies: none.

    public interface IDomainOrResponsibility : ISerializable
    {
        // Checks if this DoR includes a given entity.
        bool IsResponsibleFor(IEntity entity);
    }

    public interface IDomainOfInterest : ISerializable
    {
        // Checks if this DoI includes a given entity added event.
        bool IsInterestedInEntityAdded(EntityEventArgs args);

        // Checks if this DoI includes a given entity removed event.
        bool IsInterestedInEntityRemoved(EntityEventArgs args);

        // Checks if this DoI includes a given attribute changed event.
        bool IsInterestedInAttributeChanged(ChangedAttributeEventArgs args);
    }

    public interface IRemoteServer
    {
        // KIARA connection to the remote server.
        IConnection Connection { get; }

        // Remote domain-of-reponsibility.
        IDomainOrResponsibility DoR { get; }

        // Remote domain-of-interest.
        IDomainOfInterest DoI { get; }

        // Events which are triggered when the remote DoI or DoR has changed.
        event EventHandler DoIChanged;
        event EventHandler DoRChanged;
    }

    public interface ILocalServer
    {
        // KIARA service on the local service.
        ILocalService LocalService { get; }

        // Local domain-of-reponsibility.
        IDomainOrResponsibility DoR { get; set; }

        // Local domain-of-interest.
        IDomainOfInterest DoI { get; set; }

        // Events which are triggered when the local DoI or DoR has changed.
        event EventHandler DoIChanged;
        event EventHandler DoRChanged;
    }

    public class ServerEventArgs : EventArgs
    {
        public ServerEventArgs(IRemoteServer server)
        {
            Server = server;
        }

        public IRemoteServer Server { get; private set; }
    }

    public interface IServerSync
    {
        // Collection of remote servers.
        IEnumerable<IRemoteServer> RemoteServers { get; }

        // Local server.
        ILocalServer LocalServer { get; }

        // Events when a server is added or removed from the RemoteServers collection.
        event EventHandler<ServerEventArgs> AddedServer;
        event EventHandler<ServerEventArgs> RemovedServer;
    }

    public static class ServerSync
    {
        public static IServerSync Instance;

        public static IEnumerable<IRemoteServer> RemoteServers
        {
            get
            {
                return Instance.RemoteServers;
            }
        }

        public static ILocalServer LocalServer
        {
            get
            {
                return Instance.LocalServer;
            }
        }

        public static event EventHandler<ServerEventArgs> AddedServer
        {
            add
            {
                Instance.AddedServer += value;
            }
            remove
            {
                Instance.AddedServer -= value;
            }
        }

        public static event EventHandler<ServerEventArgs> RemovedServer
        {
            add
            {
                Instance.RemovedServer += value;
            }
            remove
            {
                Instance.RemovedServer -= value;
            }
        }
    }

    #endregion

    #region Console

    // Plug-in depedencies: none.
    // Component dependencies: none.

    public struct CommandInfo
    {
        public string ShortHelp;
        public string LongHelp;
        public bool Interactive;
        public Action<string> Handler;
        public IEnumerable<string> Aliases;
    }

    public interface IConsole
    {
        // Use these methods instead of the Console.Write* to allow correct handling of the command line.
        void WriteLine();
        void WriteLine(string line);

        // Registers a new command.
        void RegisterCommand(string name, CommandInfo commandInfo);
    }

    public static class Console
    {
        public static IConsole Instance;

        public static void WriteLine()
        {
            Instance.WriteLine();
        }

        public static void WriteLine(string line)
        {
            Instance.WriteLine(line);
        }

        public static void RegisterCommand(string name, CommandInfo commandInfo)
        {
            Instance.RegisterCommand(name, commandInfo);
        }
    }

    public class ConsoleExample
    {
        public void Example()
        {
            Console.WriteLine("Hello, world!");

            Console.RegisterCommand("print42", new CommandInfo {
                ShortHelp = "prints 42",
                LongHelp = "prints the answer to the question of life the universe and everything",
                Interactive = false,
                Handler = delegate(string commandLine) { Console.WriteLine("42"); },
                Aliases = new List<string>()
            });
        }
    }

    #endregion
}
