[![License badge](https://img.shields.io/badge/license-LGPLv3-blue.svg)](https://opensource.org/licenses/LGPL-3.0)
[![Documentation badge](https://readthedocs.org/projects/fives/badge/?version=latest)](http://fives.readthedocs.org/en/latest/?badge=latest)
[![Docker badge](https://img.shields.io/docker/pulls/tospie/fives.svg)](https://hub.docker.com/r/tospie/fives/)
[![Support badge]( https://img.shields.io/badge/support-sof-yellowgreen.svg)](http://stackoverflow.com/questions/tagged/synchronization-fives)

# FiVES

Flexible Virtual Environment Server

The goal of this project is to build a flexible virtual world server that can be used to build various synchronized applications with different requirements. Its architecture is highly modular, providing a lightweight base application with a very flexible plugin mechanism.

__*FiVES is part of the FIWARE project, funded by the European Union. There, FiVES is provided as alternative implementation of the 'Synchronization' Generic Enabler. For more information, please refer to  [https://www.fiware.org/](https://www.fiware.org/)*__

* FIWARE Catalogue entry: [Synchronization / FiVES](http://catalogue.fiware.org/enablers/synchronization-fives)
* [User & Programmer Guide](user_guide.md)
* [Installation & Admin Guide](installation_guide.md)

# Project Structure

## Directories

The following directories exist in the repository:

<table>
  <tr>
    <th>Directory</th>
    <th>Purpose</th>
  </tr>
  <tr>
    <td>Core</td>
    <td>contains core libraries for the server</td>
  </tr>
  <tr>
    <td>doc</td>
    <td>contains files for generating documentation</td>
  </tr>
    <tr>
    <td>Misc</td>
    <td>different tools for testing</td>
  </tr>
  <tr>
    <td>FIVES</td>
    <td>contains the server application</td>
  </tr>
  <tr>
    <td>Plugins</td>
    <td>contains server plugins</td>
  </tr>
  <tr>
    <td>ServiceBus</td>
    <td>Contains the Plugin-Service-Orchestration bus</td>
  </tr>
  <tr>
    <td>ThirdParty</td>
    <td>contains 3rd-party libraries not available in nuGet</td>
  </tr>
  <tr>
    <td>WebClient</td>
    <td>contains Web-client for testing the server</td>
  </tr>
</table>

The following directories may be generated dynamically:

<table>
  <tr>
    <th>Directory</th>
    <th>Purpose</th>
  </tr>
  <tr>
    <td>Binaries</td>
    <td>
      contains compiled plugins and protocols with all required third-party
      dependencies and debug files. New versions of plugins and protocols are
      automatically copied here. The FiVES application automatically loads
      plugins and protocols from this directory. This makes the edit-compile-test
      cycle easier.
    </td>
  </tr>
  <tr>
    <td>packages</td>
    <td>created by nuGet to contain 3rd party libraries</td>
  </tr>
  <tr>
    <td>test-results</td>
    <td>created by MonoDevelop to contain unit test results</td>
  </tr>
</table>

## Overall Architecture

The FiVES project consists of three main parts:

* The core server application that maintains the world data in an entity-component-attribute like fashion and loads and schedules the set of plugins. (Contained in this Repo, __Core__ folder).
* A set of plugins that implement specific logic. Whenever you want to enrich your application with new features, these features will be provided by one or more plugins. The Plugins folder in this project contains a base set of plugins that may be helpful to get started with creating interactive 3D virtual world applications. An additional set of more specialised, experimental plugins is published separately at __https://github.com/fives-team/fives-experimental-plugins.__
* The communication middleware _SINFONI_. In short, SINFONI aims at simplifying network communication by abstracting from data serialization, marshalling and transport, and hides all this behind a simple, RPC like interface. SINFONI is modular and provides a set of serialization and transport mechanisms. SINFONI is provided in FiVES as precompiled binaries in the ThirdParty/SINFONI folder. The complete source code is provided under LGPL v3 license at https://github.com/tospie/SINFONI
 
## Third-party libraries

We use nuGet to manage third-party libraries. If you want to build FIVES, please
install nuGet into your IDE and restore all packages. Some libraries that are
not available in nuGet are located in ThirdParty directory.

# Getting Started

## Building and running FiVES

All third party libraries that are needed to build FiVES are either supplied within the project, or managed by the NuGet package manager: http://www.nuget.org . Simply add NuGet to your IDE, select to restore packages on build, and build the entire solution.

FiVES can be built and run on both Windows and Linux system.

To run FiVES, just double click _FIVES.exe_ (Windows), or use mono to run FiVES (Linux) by typing `mono FIVES.exe` .
FiVES requires Administrator rights under Windows to be allowed to open HTTP listener ports.

## Quick Start Guides

There are currently the following resources that allow you to directly dive into the FiVES development:

* The FIWARE User and Programmer Guide for the FiVES Synchronization GE Implementation:  [User & Programmer Guide](user_guide.md)
* The following Google Document with a documentation that is kept rather up to date: https://docs.google.com/document/d/163_K5boYohByu8qkdBUxk8Hrp_7QVY5NTzVEzFOCZEQ
* The FiVES GitHub Wiki: https://github.com/fives-team/FiVES/wiki _This Wiki is currently outdated and under maintenance. It will be back in a revised version soon! Please refer to the other resources in the meantime_

# License

FiVES is provided subject to terms of the GNU LGPL v3 license. Please refer to the LICENSE file for more information. All third-party libraries come with their own licenses. Details about third party licenses are given in the Readme file in the third party folder, and on the project webpages of the different projects.
