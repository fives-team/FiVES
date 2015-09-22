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
