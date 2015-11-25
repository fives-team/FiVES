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

(function(){
    "use strict";

    var _communicator = FIVES.Communication.FivesCommunicator;

    var component_interface = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._registerSinfoniFunctions.bind(this));
    };

    var c = component_interface.prototype;

    c._registerSinfoniFunctions = function() {

        this.registerComponent = _communicator.connection.generateFuncWrapper("component.registerComponent");
    };

    c.initializeComponent = function(componentName) {
        return { Name: componentName, Attributes: [], addAttribute: addAttribute};
    };

    var addAttribute = function(attributeName, sinfoniType, defaultValue)
    {
        var attr = {Name: attributeName, Type: sinfoniType};
        if(defaultValue)
            attr.DefaultValue = defaultValue;
        this.Attributes.push(attr);
    };

    FIVES.Plugins.ComponentInterface = new component_interface();
}());

