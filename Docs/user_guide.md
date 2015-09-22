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

````
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
 <img src="https://github.com/fives-team/FiVES/blob/develop/Docs/eca.jpg"></img>
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
