FiVES
=====

Flexible Virtual Environment Server

The goal of this project is to build a flexible virtual world server that can
be used to build various virtual world applications with diverse requirements.

Directories
===========

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
    <td>Protocols</td>
    <td>contains KIARA protocols</td>
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

Third-party libraries
=====================

We use nuGet to manage third-party libraries. If you want to build FIVES, please
install nuGet into your IDE and restore all packages. Some libraries that are
not available in nuGet are located in ThirdParty directory.

License
=======

Please read LICENSE file. All third-party libraries come with their own
licenses. Please refer to respective projects' websites for more information.
