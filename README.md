# FiVES

Flexible Virtual Environment Server

The goal of this project is to build a flexible virtual world server that can be used to build various synchronized applications with different requirements. Its architecture is highly modular, providing a lightweight base application with a very flexible plugin mechanism.

__*FiVES is part of the FIWARE project, funded by the European Union. There, FiVES is provided as alternative implementation if the 'Synchronization' Generic Enabler. For more information, please refer to http://www.fiware.org*__



# Directories

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
    <td>Docs</td>
    <td>contains files for generating documentation</td>
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
      automatically copied here and FiVES application automatically loads
      plugins and protocols from this directory. This makes edit-compile-test
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

The Plugins folder in this project contains a base set of plugins that may be helpful to get started with creating interactive 3D virtual world applications. An additional set of more specialised, experimental plugins is published separately at https://github.com/fives-team/fives-experimental-plugins.

# Third-party libraries


We use nuGet to manage third-party libraries. If you want to build FIVES, please
install nuGet into your IDE and restore all packages. Some libraries that are
not available in nuGet are located in ThirdParty directory.

# License

FiVES is provided subject to terms of the GNU LGPL v3 license. Please refer to the LICENSE file for more information. All third-party libraries come with their own licenses. Details about third party licenses are given in the Readme file in the third party folder, and on the project webpages of the different projects.
