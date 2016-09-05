// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License version 3
// (LGPL v3) as published by the Free Software Foundation.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU LGPL License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};


/**
 * COMPONENT INTERFACE PLUGIN
 *
 * This client-side can be used to communicate with the server-side counterpart "Component Interface" . Component
 * Interface exposes the API to register new components for clients to register new component types during run-time
 *
 * Usage:
 * - Initialize a new component definition object (FIVES.Plugins.ComponentInterface.initializeComponent)
 * - Add Attribute definitions using the respective method on the returned component definition object
 * - call the remote procedure (FIVES.Plugins.ComponentInterface.RegisterComponent) with the component object as
 *    parameter
 *
 */

(function(){
    "use strict";

    var _communicator = FIVES.Communication.FivesCommunicator;

    var component_interface = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._registerSinfoniFunctions.bind(this));
    };

    var c = component_interface.prototype;

    c._registerSinfoniFunctions = function() {
        /**
         * Remote Procedure Call to the server-side SINFONI service to register a new component. Uses a previously
         * initialized component object with a list of added attributes as parameter
         * @param definition ComponentDefinition object
         */
        this.registerComponent = _communicator.connection.generateFuncWrapper("component.registerComponent");
    };

    /**
     * Initializes a new component object
     * @param componentName Unique name of the new component
     * @returns {{Name: *, Attributes: Array, addAttribute: Function}} Initialized component object
     */
    c.initializeComponent = function(componentName) {
        return { Name: componentName, Attributes: [], addAttribute: addAttribute};
    };

    /**
     * Adds a new Attribute definition to a previously initialized component definition object
     * @param attributeName Unique name of the attribute within the component
     * @param sinfoniType Name of the Type in the SINFONI IDL for the new attribute
     * @param defaultValue (optional) Default value of the new attribute
     */
    var addAttribute = function(attributeName, sinfoniType, defaultValue)
    {
        var attr = {Name: attributeName, Type: sinfoniType};
        if(defaultValue)
            attr.DefaultValue = defaultValue;
        this.Attributes.push(attr);
    };

    FIVES.Plugins.ComponentInterface = new component_interface();
}());

