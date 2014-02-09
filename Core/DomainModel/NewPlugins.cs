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

    public interface IAreaOrResponsibility : ISerializable
    {
        // Checks if this AoR includes a given entity.
        bool IsResponsibleFor(IEntity entity);
    }

    public class ChangedAoREventArgs : EventArgs
    {
        public ChangedAoREventArgs(string pluginName, IAreaOrResponsibility aor)
        {
            PluginName = pluginName;
            AoR = aor;
        }

        public string PluginName { get; private set; }
        public IAreaOrResponsibility AoR { get; private set; }
    }

    public interface IAoRCollection
    {
        // Provides interface to change the AoR of a plug-in.
        IAreaOrResponsibility this[string pluginName] { get; set; }

        // Event which is triggered when the AoR has changed.
        event EventHandler<ChangedAoREventArgs> Changed;
    }

    public interface IAreaOfInterest : ISerializable
    {
        // Checks if this AoI includes a given entity added event.
        bool IsInterestedInEntityAdded(EntityEventArgs args);

        // Checks if this AoI includes a given entity removed event.
        bool IsInterestedInEntityRemoved(EntityEventArgs args);

        // Checks if this AoI includes a given attribute changed
        // event.
        bool IsInterestedInAttributeChanged(
            ChangedAttributeEventArgs args);
    }

    public class ChangedAoIEventArgs : EventArgs
    {
        public ChangedAoIEventArgs(string pluginName, IAreaOfInterest aoi)
        {
            PluginName = pluginName;
            AoI = aoi;
        }

        public string PluginName { get; private set; }
        public IAreaOfInterest AoI { get; private set; }
    }
    
    public interface IAoICollection
    {
        // Provides interface to change the AoI of a plug-in.
        IAreaOfInterest this[string pluginName] { get; set; }

        // Event which is triggered when the AoI has changed.
        event EventHandler<ChangedAoIEventArgs> Changed;
    }

    public interface IServer
    {
        // KIARA connection to the remote server.
        IConnection Connection { get; }

        // Remote collection of the areas-or-reponsibility.
        IAoRCollection AoR { get; }

        // Remote collection of the areas-or-interest.
        IAoICollection AoI { get; }
    }

    public static class ServerSync
    {
        // Collection of remote servers.
        public static IEnumerable<IServer> RemoteServers;

        // Local server.
        public static IServer LocalServer;
    }

    #endregion

    #region Terminal

    // Plug-in depedencies: none.
    // Component dependencies: none.

    public interface ICommandInfo
    {
        string ShortHelp { get; }
        string LongHelp { get; }
        bool Interactive { get; }
        Action<string> Handler { get; }
        IEnumerable<string> Aliases { get; }
    }

    public interface ITerminal
    {
        // Use these methods instead of the Console.Write* to allow correct handling of the terminal command line.
        void WriteLine();
        void WriteLine(string line);

        // List of registered commands. Returned command infos are read only.
        IEnumerable<ICommandInfo> RegisteredCommands { get; }

        // Creates new command info.
        ICommandInfo CreateCommandInfo(string shortHelp, string longHelp, bool interactive, Action<string> handler,
            IEnumerable<string> aliases);

        // Registers a new command.
        void RegisterCommand(string name, ICommandInfo commandInfo);
    }

    public static class Terminal
    {
        public static ITerminal Instance;
    }

    #endregion
}
