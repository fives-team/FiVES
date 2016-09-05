# Introduction

This document describes the use of the alternative implementation "FiVES" of the Synchronization Generic Enabler,
both the server and client parts.
The typical use of this GE is creating multi-user networked scenes, 
which may include 3D visualization (for example a virtual world or a multiplayer game). A client connects, using the
WebSocket protocol, to a server where the multi-user scene is hosted, after which they will receive the scene content
and any updates to it, for example objects' position updates under a physics simulation calculated by the server.
The clients interact with the scene on the server by a remote-procedure-call based approach. 

Please note that FiVES is an alternative implementation to the Synchronization GE Reference Implementation. Even though FiVES follows this specification and implements the same reference architecture, not all parts of both are yet available. Missing parts are subject to future updates. 

# User Guide

The user guide section describes on a high level how FiVES is configured and started, and how the 3D-UI-XML3D based Web Client connects to the server. 

## Data Model

FiVES uses the same Entity-Component-Attribute data model as the Synchronization GE reference implementation "realXtend Tundra". It is described here: https://github.com/realXtend/tundra/wiki/Scene-and-EC-Model
For a detailed information about how data is represented and which actions can be performed on data, please refer to the Synchronization GE User and Programmer's guide. 

## Using the FiVES server

The FiVES Synchronization framework is a complex piece of software. A full and detailed description of how it can be used and configured is beyond the scope of this document.
Instead, this User Guide will give a quick intro of how to setup and use the out-of-the-box FiVES distribution as it comes with this repo.

After having completed installation of FiVES as described in the ''FiVES - Installation and Administration Guide'', FiVES can simply be started by double clicking FiVES.exe (Windows) or stating FiVES via the command

 ``mono FiVES.exe``

in Linux. FiVES will start up prompting the FiVES server console:
<pre>
The server is up and running. Use ‘quit’ command to stop it
>>
</pre>

For a full list of available console commands, type "help", "h" or "?".

## Configuring the FiVES Server

The prebuilt binaries as well as the solution for FiVES sources is preconfigured to provide a FiVES setup for synchronized 3D worlds with animated avatars upon first launch, using most of the Plugins that come with FiVES. For your own application, though, you may want to disable some of the plugins, add your own, or adapt network configuration to work in your specific network. 

### Configuring loaded Plugins

The list of plugins that should be loaded is specified in the FiVES core project application configuration file. In the IDE, this file is listed as ``app.cfg`` in the FiVES project folder. After the build process, it will appear as ``FIVES.exe.cfg`` in the Binaries folder.
This file contains a region _appSettings_ : 

```
<appSettings>
  <add key="PluginDir" value="." />
  <add key="PluginBlackList" value=" … " />
  <add key="ProtocolDir" value=" … " />
  <add key="ProtocolWhiteList" value=" … "/>
  <add key="ServerIDL" value="http://localhost:8181/fives/fives.kiara/"/>
</appSettings>
```
* _PluginDir_: The directory from which plugins are loaded, relative to the FIVES.exe binary file. For convenience, the initial configuration is done such that Plugins and executable are located in the same folder
* _PluginBlackList_: A list of comma separated names of Plugins that should NOT be loaded when the server is starting
* _PluginWhiteList_: A list of comma separated names of Plugins that should ONLY be loaded when server is starting. * PluginWhitelist is ignored if the field for PluginBlacklist is present in the config and contains any Plugins!
* _ProtocolDir_ and _ProtocolWhiteList_ do not need to be changed in the current version of FiVES
* _ServerIDL_: Location from which the IDL defining the FiVES datastructures and SINFONI services should be retrieved from. Make sure the host under which the IDL is provided matches the host specified for FiVES in the Network Configuration! (See next section) 

### Network Configuration

FiVES uses the SINFONI, an adapted version of the KIARA middleware, for Server-Client and Inter-Server-Communication. This document will not dive into the details of KIARA or SINFONI configuration. In fact, no changes to this module are needed to use FiVES as Synchronization GE in FIWARE.

The place to look for network configuration is the file ``SINFONIPlugin.dll.config`` in the Binaries folder, or the ``app.cfg`` file of the _SINFONIPlugin_ project within the Plugins folder of the FiVES solution in the IDE. The configuration file looks like this:

```
<configuration>
  <ServerConfiguration>
    <!-- Specifies the listener to which clients connect in order retrieve …  -->
    <!-- This server configuration document will contain the URI to the IDL as well … -->
    <!-- the implemented services according to the KIARA specification -->
    <ConnectionListener host ="+" port="8181" path="/fives/" />

    <!-- FiVES will host all Plugin-provided service under a combined service in … -->
    <!-- specification. This service will run on the same IP as the server as …  -->
    <ServiceConfiguration host="Any" transport="ws" protocol="fives-json" port="34837" />
  </ServerConfiguration>
  <Paths>
    <ProtocolPath value = "./SINFONI" />
    <TransportPath value = "./SINFONI" />
  </Paths>
</configuration>
```
* ConnectionListener: The end point at which FiVES listens for incoming client or remote server node connections.
  * host: IP address or hostname of the machine running FiVES. If FiVES should be exposed in a public network, this host address must be the IP under which the machine can be accessed in the network, NOT localhost. Also use the same host address as IDL URL in the overall FiVES configuration (see previous section).
  * port: Port under which FiVES listens for incoming connections
  * path: Path under which FiVES will provide server information upon incoming connection. 

* ServiceConfiguration: Configures the channel that the FiVES service that is started within SINFONI uses for real time communications.
  * port: The port that is used by the real time communication channel.
The remaining fields, transport and protocol, do not need to be changed for a deployment of FiVES in FIWARE. 

### Configuring individual Plugins

In addition to the application wide configuration above, there are also some plugins which allow to be configured individually. Examples are for example StaticScenery or EventLoop. Watch out for any configuration file app.cfg in the individual project folders of the respective plugins in the IDE, or for configuration files [pluginname].dll.cfg in the Binaries folder. The options and values in these config files are rather self explanatory. 

## Interacting with the Web Client

After the Web client was deployed and configured, it can simply be accessed via your Web browser of choice - preferably Google Chrome - via

```
http://$[Path To Web Client Folder On Server]/client.xhtml
```

In the default deployment, with Authentication plugin loaded, FiVES expects the user to identify with Username and Password. Please note that the provided authentication Plugin does not yet implement a real check of username and password! Username is just used to assign an avatar to a user, password is ignored.

Having signed in, you will see the Avatar entity created for you (a small fire truck in the provided sample setup) that can be controlled via the W, A, S, D keys. 
# Programmer Guide

The programmer's guide describes how the FiVES Plugin model can be used on both server and client side to add new features to a virtual world applications. It explains how to introduce new components on server side, and how to add scripts to the client that operate on this data. Please note that we can only give a first insight to the mechanics of FiVES. Please check the FIWARE E-Learning platform regularly, where we plan to publish new courses on the different FiVES mechanics regularly. 

## The FiVES ECA Model
FiVES represents the World in the so called Entity-Component-Attribute-Model (or ECA), which is not only known from object-based databases, but is also used in popular game engines, such as Unity.

In this representation, each object in the world is considered to be an Entity. Everything is an Entity, and in turn, an Entity can basically be anything. An Entity as such does not fulfill a special role in the world, but is rather a container for a set of Components.

A Component is a data structure that in turn forms a container for a set of Attributes. Attributes can in turn be considered as a kind of typed Variables. Which attributes - and of which type - are contained within a component is defined in a ComponentDefinition object that is registered to the CompontentRegistry of FiVES. For more information of how to register and access Components, please refer to the sections _Components and Attributes_ and _Registering Components_.

The key message to take home at this point is: What role an Entity plays in your world is entirely determined by what components it carries, and by what values are stored in the attributes. All the actual data is contained in some attribute, and all synchronization of data between FiVES and connected applications is solely done by synchronising attribute values (and which entities are part of the World, of course). 

<center>
 <img src="https://github.com/fives-team/FiVES/blob/develop/doc/img/eca.jpg?raw=true"></img>
</center>

### Entities and the World

Basically anything that exists in a FiVES world is an Entity. There is no assumption about what exactly may be represented by an Entity from the beginning, so you are completely free to decide what to turn your entities into.

You want your users to be able to be roam the world as animated avatars? Then an avatar will possibly be an entity. That trees and rocks and houses there? All of them entities! What an entity represents is basically determined by what components are attached to it, and by which values are stored in the respective attributes. More about components and attributes will follow in the next section.Entities are simply created by using the default constructor (no parameters):

```
   // Create a new entity
   Entity myEntity = new Entity();
```

Note that simply creating an entity is not enough to make it part of your FiVES World! Newly created entities just exist in some kind of Nirvana, unaware of the world around, and the world around unaware of them. This also means that all operations on components and attributes performed on an entity will not be subject to modifications performed by the FiVES Plugin Bus. For more information on the FiVES Plugin Bus, please refer to section 5.3 The FiVES Plugin Bus (ServiceBus) in the detailed documentation: <a href ="https://docs.google.com/document/d/163_K5boYohByu8qkdBUxk8Hrp_7QVY5NTzVEzFOCZEQ/edit#heading=h.yewfc6swszum">FiVES Plugin Bus</a>.

The set of all entities present in the current FiVES world is called World. Within this World, each entity is identified by a Global Universal Identifier (Guid) that is assigned automatically as soon as an entity is created. In addition, each Instance of a World is assigned a Guid, too, that is also stored within the Entity as Owner to be able to determine where an entity came from in a setup of multiple distributed FiVES instances. World can be accessed from your code via FIVES.World.Instance. FIVES.World.Instance provides the following functions:

```
   // Add an entity to the World
   public void Add(Entity entity);
  
   // Remove an entity from the world. Returns true if entity was removed successfully
   public bool Remove(Entity entity);
  
   // Check if World contains an entity
   public bool ContainsEntity(Guid guid);
  
   // Return an entity that is part of the world
   public Entity FindEntity(string guid)
   //or
   public Entity FindEntity(Guid guid)
```

### Component and Attributes

Components and the Attributes that are contained within a component are defined by Plugins. Let’s take the example of a Plugin that allows to describe 3D position of entities, which we already used earlier: This plugin would register a component called “location”, containing two attributes, “position” and “orientation”, of types Vector and Quaternion respectively. How Plugins can register new components is described in section #Registering Components.

Both Components and Attributes are identified by a name, comparable to variable names in programming or scripting languages. Once they are registered, they can be accessed on an entity by square bracket operators [ ], for example:

```
   var myPosition = (Vector) entity[“location”][“position”];
```

This returns the current value of the entity’s position, that is stored in some component called “location”. Note that having read the attribute value, it needs an explicit cast to the target type.

When writing an attribute value, we are again facing a specialty of FiVES. In fact, we cannot set attribute values directly, but we are suggesting a new value for a given attribute:

```
   entity[“location”][“position”].Suggest( new Vector(0, 0, 1) );
```

This may look a bit nasty at the beginning: Why can’t I just set the value I want to set? How can I be sure that my suggestion really makes it into the attribute? There are actually two reasons for this: First one occurs from the fact that different plugins are usually self-contained and unaware of each other. Because of this, FiVES uses a Plugin Bus to define execution order of Plugins, and while performing these execution changes, an attribute value may be changed further. This Plugin Bus is a rather complex construct and will not be tackled in this programmer's guide.

The second reason lies in the SINFONI middleware that is used in FiVES. In fact, even though attributes are typed variables, SINFONI lets you use data types to write into attributes that just “look like” the attribute type, i.e. , SINFONI is able to map from the type you are trying to write to the attribute type.

In conclusion, suggesting attributes make perfect sense: What you are basically doing is throwing one rather fitting value at the attribute, letting FiVES set the attribute value properly for you, ensuring that the value that finally makes it into the attribute matches what you would like to see there.

### Entity and Component Events

World and Entities fire C# events whenever changes are happening to keep Plugins informed about what is going on in the World. For the World, this includes events for added and removed entities. Entities fire events when Components are initialized or the value of an attribute has changed:

Events defined on World.Instance are in particular:
```
   /// <summary>
   /// Raised when a new entity has been added.
   /// </summary>
   public event EventHandler<EntityEventArgs> AddedEntity;
  
   /// <summary>
   /// Raised when an entity has been removed.
   /// </summary>
   public event EventHandler<EntityEventArgs> RemovedEntity;
```
, with EntityEventArgs containing the entity that was created, or a copy of the last state of the entity when the entity was removed.

Events defined on Entity are:
```
   /// <summary>
   /// An event that is raised when a new component is created in this entity.
   /// </summary>
   public event EventHandler<ComponentEventArgs> CreatedComponent;
  
   /// <summary>
   /// An event that is raised when any attribute in any of the components of this
   /// entity is changed.
   /// </summary>        
   public event EventHandler<ChangedAttributeEventArgs> ChangedAttribute;
  
   /// <summary>
   /// An event that is raised when a change to any attribute in any of the
   /// components was suggested.
   /// </summary>    
   public event EventHandler<ProposeAttributeChangeEventArgs> ProposedAttributeChange;
```
, with the event arguments containing information about which attribute in which component changed its value and about the new value. Note that the ProposedAttributeChange is fired as soon as a new value was suggested for the attribute for the first time, whereas ChangedAttribute is fired when an attribute was finally set to the resulting value of an attribute change.

It is strongly discouraged to write an attribute in response to an ChangedAttribute Event!! That is, don’t use ChangedAttribute in order to do something like this:

```
   e.ChangedAttribute += (o,e) => {
       e = (Entity)o;
       e[“c”][“a”].Suggest(someValue);       // <- DON’T !!
   }
```

This will always cause you trouble. First, it may lead to an infinite loop immediately if there is no check or control about which attribute was changed and which one was written. In the example above, the handler will always immediately cause a new ChangedAttribute event.

Even if you add some control to not trigger infinite events, the approach above is a bad idea, as you do not know whether another plugin may want to react to the event in a similar way. This will lead to an update conflict and in worst case to corrupted or inconsistent data.

If your plugin needs to handle a ChangedAttribute event by changing another attribute or the same attribute again, please do so by using the Plugin Bus. A respective E-Learning course about how to use the Plugin bus is currently in progress.

NOTE: Synchronization of data is currently solved by the ClientManager plugin, that basically subscribes to ChangedAttribute and sends all of these events to all connected client application. This means that you do not have to write your own code for synchronization of data. You may have to consider that there is currently no control about which attributes should be synchronized at all. This is subject to a future update. In general, we suggest to keep local data in ordinary C# variables, and only use attributes for data that should really be synchronized

### Complete Example

In the following example, we are going to take some entity in the World and create a new entity nearby:
```
   private void CreateEntityCloseTo(Guid otherEntityGuid)
   {
       // Get entity from World
       Entity other = FIVES.World.Instance.FindEntity(otherEntityGuid);
      
       // Get position of other entity
       Vector otherPos = (Vector) other[“location”][“position”];
      
       // Set new position nearby the other entity
       Vector newPos = new Vector (otherPos.x + 1.0f, otherPos.y, otherPos.z);
      
       // Create new entity
       Entity newEntity = new Entity();
      
       // Set position of entity next to other entity.
       newEntity[“location”][“position”].Suggest( newPos );
      
       // Add Entity to the World. Connected clients will be informed
       // about the new entity and receive an entity object with initial
       // attribute values
       World.Instance.Add(newEntity);
   }
```

## FiVES Plugin Development

Plugins are the core mechanism in FiVES to add new features and extend the available data structures in terms of components. The Interface that needs to be implemented is kept intentionally small to make it easy to integrate also complex new features into a FiVES application.

For this, a plugin implements one or more component definitions (see also _Registering Components_) and all the logic that makes use of this data in C# code. Using the example of the Motion plugin that we introduced before, Motion would define components to describe velocity on entities, and provide C# code to translate this velocity into continuous position and orientation changes.

### Creating a Plugin

Adding a new functionality to your FiVES application usually means creating a Plugin that implements this functionality. To create a Plugin, just follow these simple steps:

1. Create a new project of type Class Library in the folder <root>/Plugins/<plugin-name>.
2. Make sure target .NET framework is 4.0
3. Set its output directory to <root>/Binaries/<configuration>:
4. In Visual Studio:
 1. Open project properties
 2. Select Build on the left
 3. Set Configuration to Debug
 4. Set Output path to ..\..\Binaries\Debug\
 5. Set Configuration to Release
 6. Set Output path to ..\..\Binaries\Release\
 7. Save changes (Ctrl + S) 
5. In MonoDevelop
 1. Open project properties
 2. Select Build -> Output on the left
 3. Set Configuration to All Configurations
 4. Set Output path to ..\..\Binaries\$(Configuration}
 5. Click OK 
6. Set the default namespace for the plugin to <plugin-name>Plugin, e.g. ConsolePlugin.
7. Add references to the PluginManager, and optionally DomainModel, Collections and Math projects.
8. Create a class implementing IPluginInitializer:
  1. Property Name should return a unique plugin name.
  2. Property PluginDependencies should return a list of other plugin names whose functionality (classes) new plugin uses.
  3. Property ComponentDependencies should return a list of components on new plugin accesses.
  4. Method Initialize which will be called when all dependencies are satisfied. Note that plugin may also be initialized after the server is started, e.g. when a component
  5. dependency is satisfied by a plugin on a different server or a plugin dependency is satisfied using PluginManager.LoadPlugin method. 

As soon as the Plugin was compiled to a .dll and copied to the folder FiVES loads the Plugins from, it will be available when the server starts for the next time.
```
   public interface IPluginInitializer
   {
       /// The name of the plugin.
       string Name { get; }
      
       /// List of names of the plugins on whose functionality
       /// (classes) this plugin depends on.
       List<string> PluginDependencies { get; }
      
       /// List of names of the components that this plugin accesses.
       List<string> ComponentDependencies { get; }
      
       /// Initializes the plugin. This method will be called by the plugin manager
       /// when all dependency plugins have
       /// been satisfied.
       void Initialize();
      
       /// Called when the Plugin is shut down gracefully
       void Shutdown();
   }
```
The IPluginInitializer interface from which each plugin is derived. 

### Registering Components

Components that can be written on and read from entities need to be registered to the global FiVES Component Registry before they can be accessed. Please refer to #The FiVES ECA Data Model for further information of the Entity Component Attribute model that is used by FiVES, and on how to read or write components and attributes.

The idea of defining components before making them available is to provide some kind of type and name binding check. The next section will introduce the concept of plugin dependencies. Having plugins define components on which to depend explicitly avoids semantic errors in the code which may be hard to track, like typos in attribute names, or using attributes with wrong types.

Plugins introduce new Components by creating component's definition and then registering it with the ComponentRegistry. A component definition defines a set of attributes that form the component by specifying the attributes' types, names and optional default value. Attributes can be of any standard type that supports IConvertible interface. When the default value is omitted, a default value default(T) will be used instead, where T is the type of the attribute

As soon as the new component is registered, it can be accessed along with it's attributes via the square bracket operator as described above.

```
   // create new component
   ComponentDefinition meshResource= new ComponentDefinition("meshResource");
  
   // add a string attribute "uri" with default value default(string), which is null
   meshResource.AddAttribute("uri");
  
   // add a bool attribute "visible" of type bool with default value true
   meshResource.AddAttribute("visible", true);
  
   // register component
   ComponentRegistry.Instance.Register(meshResource);
```

### Plugin Dependencies

The idea behind the FiVES plugins also includes to have a set of reusable pieces of code that can be assembled to new applications. This can include reusing a component that is defined in another plugin, as well as reusing C#-Code that is exposed via a public interface of a Plugin. However, this also introduces dependencies between plugins. In the first case, the new plugin will depend on a component that was previously defined (in our old example of Motion, the plugin will depend on the components defined by Location), in the second case, the plugin will depend immediately on Plugin code. In our example Motion depends on the code of a plugin called EventLoop, that provides a very basic implementation of a simulation loop.

To make sure that the Plugin libraries are loaded in the correct order, the IPluginIntializer interface from which a new Plugin is derived contains two fields: PluginDependencies and ComponentDependencies, which describe exactly what the name is stating. If your Plugin happens to depend on another Plugin, not just a component, you will also have to link the respective Plugin library (or Project within the solution) to the resources of your new Plugin project.

Both return a List of Names of Components or Plugins. These are simply lists of strings, i.e. of all names of plugins or components the new plugin depends on. These names are case sensitive, so make sure there is no Typo, otherwise the dependency cannot be resolved correctly!

In the often mentioned case of Motion, these dependencies thus look like this:
```
    public List<string> PluginDependencies
    {
        get
        {
            return new List<string> {"EventLoop"};
        }
    }
   
    public List<string> ComponentDependencies
    {
        get
        {
            return new List<string> { "location" };
        }
    }
```

The FiVES Plugin Manager will resolve the dependencies when the server is starting. All plugins that define dependencies that are not yet loaded, they are put into a deferred state, and a new attempt to initialize them is made as soon as the other plugin got loaded.

When running FiVES with the Terminal plugin, you can type ‘plugins’ after the server finished the startup procedure. This will prompt you with a list of all successfully initialised plugins, as well as which plugins could not be initialized due to missing dependencies. 


## Web Client development

The Web Client is split into a core application and several plugin-like scripts similar to the FiVES server application. By this, implementing logic that is needed to cover features of a server side plugin on client side should be nothing more than adding a new JavaScript file that implements the respective logic. 

### The Web Client Project

The Web Client is divided into different subfolders, containing configuration files, script files and some folders for example resources for XML3D scenes. Within the script folder, you will subfolders for all the files that implement Client-Server-communication (communication), external libraries, like xml3d.js and kiara (lib), all the files that implement the FiVES ECA datamodel (model), and all the scripts that implement the client-side features of all the FiVES plugins (plugins). There are more folders, containing examples or code for e.g. user input.

The overall namespace that we are using the Web Client is FIVES. Each of the above modules - communication, domain model, plugins … - build their own sub namespace, e.g. FIVES.Models. Convention is to model these namespaces at the beginning of each file using JavaScript objects:

```
   var FIVES = FIVES || {};
   FIVES.Models = FIVES.Models || {};
```

Code within a script file should be contained in a JavaScript function closure and enforce strict mode:

```
   (function (){
        "use strict";
             
       // YOUR CODE HERE
   }());
```

When receiving an entity from the server that carries a reference to XML3D geometry in the mesh component, XML3D elements are created automatically that describe model and transformation of the respective entity. These element are also accessible on the entity’s JavaScript object representation via entity.xml3dView. 

### Adding features to the web client

New Features are usually added because data that emerges from some new plugin on server side needs to be interpreted correctly by the client. This logic that interprets a server side plugin should be added as single script file, following the above conventions, being contained in a subfolder of the plugins folder. Please have a look at existing script files for reference.

Similar to the Server, all entities are contained in a shared set, called FIVES.Models.EntityRegistry, and entities are queried from this set via

```
   FIVES.Models.EntityRegistry.getEntity( entityGuid );
```

Components are accessed on entities like on server side with square bracket operators:
```
   var entityPosition = e[“location”][“position”];
```

Note that setting attributes directly in the client is strongly discouraged, as there is no immediate update mechanism implemented on client side yet! Instead, please update attributes of entities directly on the server, using a SINFONI function wrapper. How to do this will be explained in more detail in #Connecting to FiVES with SINFONI.

To add your plugin script to the list of scripts used when starting the client, you have to add a script reference to the client.xhtml file:
```
   <script type="text/javascript" src="scripts/plugins/myPlugin/plugin.js"></script>
```

Make sure to include your script after having included other plugins on which your new plugin may depend, and in particular after the core scripts from models and communication! There is no dependency resolution of plugins like on server side yet. 

### Web Client Events

The client offers a number of events for plugins to subscribe to in order to be informed about changes that happened. These events are implemented in /scripts/communication/fives_event.js and are in detail:

* FIVES.Events.ConnectionEstablished()
* FIVES.Events.ComponentUpdated(entity, componentName,attributeName)
* FIVES.Events.EntityAdded(entity)
* FIVES.Events.EntityGeometryCreated(entity) 

Connection established: Fires when the connection to the server was established successfully. This event should be used to to register SINFONI Function Wrappers after a successful connection

Component updated: Fired whenever new data for a component arrived. The event calls the event handler with the entity on which the update happened, and names of component and attribute that changed. When the event is fired, the update was already applied to the attribute, and the new value is already contained there.

EntityAdded: Is fired when a new entity was received from the server and added to the entity registry. The handler for this event is fired with the newly added entity as argument.

EntityGeometryCreated: Is fired when the XML3D geometry for an entity was created successfully, that is, as soon as the referenced model was retrieved from the remote repository, elements for transformation and model are created, and the elements were added to the website’s DOM.

Plugins can register to the different events by using the .Add[Event]Handler on FIVES.Events, for example

```
   FIVES.Events.AddOnComponentUpdatedHandler( _myHandler );
```

, with _myHandler being some JavaScript function that expects the arguments with which the handler function is called as arguments. Plugins can unregister from events by using the respective FIVES.Events.Remove[Event]Handler.

### Connecting to FiVES with SINFONI

The interface for SINFONI functions is provided by the FIVES.Communication.FivesCommunicator object. The main two functions you will be using here are .generateFuncWrapper and .registerFuncImplementation that are provided by the connection object of the communicator, and only available after the connection to the server was established successfully.

First one is used to create an RPC call to the server. For example:
```
   var c = _fivesCommunicator.connection;
   var updatePosition = c.generateFuncWrapper("location.updatePosition");
```
The above code creates an object that can be used to send RPC calls to the FiVES server and stores this object in the variable updatePosition. updatePosition can then be called like a function, with parameters that match to the parameters specified in the FiVES IDL document for the respective function:
```
   updatePosition(“123aef”, {x: 1.0, y: 0.0, z: 0.0}, 12345);
```
Note that the service function that is wrapped, in this case location.updatePosition, needs to be implemented by some plugin on server side and appear in the server’s IDL document. If the function you are wrapping is a one-way function, i.e. of return type void you are done. If you are expecting some return value, though, the above line will return a function call object that will trigger a success on error event, depending on the outcome of the call. This can be handled by registering an event handler, for example:
```
   // call some authentication function wrapper that calls
   // an authentication service on server side
   var authRequest = auth(“myName”, “myPassword”);
   authRequest.on(“success”, function(result) { // handle result });
```
Providing an implementation of a service function that can be wrapped by the server is done via
```
   Connection.registerFuncImplementation(qualifiedMethodName, typeMapping, nativeMethod)
```
This binds a KIARA (SINFONI) service description as given in the IDL document to a JavaScript function implementation. typeMapping is currently ignored, qualifiedMethodName is build from [service].[method], e.g. “location.updatePosition”. The local JavaScript implementation is passed as parameter for nativeMethod . For example:
```
   registerFuncImplementation("objectsync.receiveObjectUpdates", null, _applyUpdates);
```
, binds the local JavaScript function _applyUpdates to the SINFONI (KIARA) service function objectsync.receiveObjectUpdates. The FiVES server can now wrap this method and perform a remote procedure call from server to client.

Important Note: There is NO automatic update of attributes from client to server, as it is for the other direction. Currently, if you would like your client to have attributes that are defined by your client, on the server, you need add a SINFONI service to your server side plugin that allows you to change the new attribute. The client then needs to wrap this function and call the particular setter when changing this attribute, as it is for example the case for location.updatePosition in the example above.

Another Important Note: The JavaScript binding of KIARA / SINFONI does currently not parse the IDL correctly. This needs to be tackled urgently in one of the next updates! Due to some conveniences that JavaScript brings, features like type mapping work nevertheless (as there are no types in JavaScript). And if something goes wrong, server side SINFONI still catches and handles the error. But: JS Sinfoni is also not capable to determine a method’s return type, in particular, whether a function is one way or not. Calling one way functions from server on client side will cause KIARA errors claiming Invalid CallIDs! The reason is simply that the client is not aware that its function is one way, and tries to send a reply for a method that should not. You can either ignore the KIARA Call errors, or add them manually to the list of all one way functions. For this, open the file ``scripts/lib/websocket-json.js``. Find the function ``Connection.prototype._isOneWay`` and add all functions that are one way and implemented on clientside to this list in the form ``servicename.methodname`` . 

### FiVES Scene API

The client-side plugins provide wrappers to the server-side SINFONI services that can be used to interact with the scene. This includes creation of entities, updating their positions and orientations, and some more. Following are the most common functions that are used to interact with a FiVES scene: 

#### Entities
```
   // Returns a list of all entities currently maintained by the server   
   FIVES.Plugins.ClientSync.listObjects();
  
   // Creates an entity at specified position. Returns GUID of new entity upon successful creation
   FIVES.Plugins.Editing.createEntityAt(Vector position);
   
   // Creates an entity at respective position and orientation that is rendered using
   // the given mesh resource. Returns GUID of new entity upon successful creation
   FIVES.Plugins.Editing.createMeshEntity(Vector position, Quat rotation, Vector scale, MeshResource mesh);
```

#### Position, Orientation and Velocity

The client-side plugin "Location" provides functions to send position and orientation updates for an entity to the server:
```
   // Update position of an Entity on the server
   FIVES.Plugins.Location.sendEntityPositionUpdate(string entityGuid, Vector newPosition);
   
   // Update orientation of an Entity on the server
   FIVES.Plugins.Location.sendEntityOrientationUpdate(string entityGuid, Quat newOrientation);
```
The client-side pluginmotion can be used to update velocity and rotational velocity on an entity. Setting these values to something different from a null vector will start a continuous motion, i.e. position and orientation updates, on the respective entity.
```
   // Updates values of velocity and rotational velocity of an entity. Timestamp specifies the time when the update occurred.
   FIVES.Plugins.Motion.updateMotion(string guid, Vector velocity, AxisAngle rotVelocity, i32 timestamp);
```

#### Animation

The Plugin KeyframeAnimation provides a service animation to manage animation playback on entities. Animations can either be running on server side, creating new keyframe values which are then synchronized to the clients, or just send a message to every client to invoke a client side animation playback there. In this case, animations will be running on each client, but animations may not be necessarily be in synch w.r.t. keyframes. However, client side animation playback reduces the number of messages sent over network significantly.
```
   // Starts an Xflow animation with a given name on specified entity, playing animation in specified key range for number of cycles.
   // Choose 0 for cycles for infinite playback.
   FIVES.Plugins.Animation.startServersideAnimation(string entityGuid, string animationName, float startFrame, float endFrame, i32 cycles, float speed);
         
   // Stops animation on specified entity
   FIVES.Plugins.Animation.stopServersideAnimation(string entityGuid, string animationName);
      
   // Invokes playback of animation with given name on all connected clients
   FIVES.Plugins.Animation.startClientsideAnimation(string entityGuid, string animationName, float startFrame, float endFrame, i32 cycles, float speed);
      
   // Stops client side playback of named animation on all clients
   FIVES.Plugins.Animation.stopClientsideAnimation(string entityGuid, string animationName);
```

#### Data Structures used for Synchronization

* Vector: 3D Vector with components x, y and z of type float 

```
   struct Vector {
       float x;
       float y;
       float z;
   }
```
* Quat: Quaternion to describe orientations and rotations. 
```
   struct Quat {
       float x;
       float y;
       float z;
       float w;
   }
```
* AxisAngle: Rotation in Axis Angle - notation, used to describe rotational velocity of an entity 
```
   struct AxisAngle {
       Vector axis;
       float angle;
   }
```
* MeshResource: Small container class that contains information about used XML3D resource and visibility flag 
```
   struct MeshResource {
       string uri;
       boolean visible;
   }
```
