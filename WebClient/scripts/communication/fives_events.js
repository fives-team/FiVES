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
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.

var FIVES = FIVES || {};
FIVES.Events = FIVES.Events || {};

(function () {
    "using strict";

    /**
     * List of functions that are executed when an entity fires an attribute changed event
     * @type {Array}
     * @private
     */
    var _onComponentUpdatedHandler = [];

    /**
     * List of handlers that are executed when entity added event was fired
     * @type {Array}
     * @private
     */
    var _onEntityAddedHandler = [];

    /**
     * List of handlers that are executed when entity finishes creating its XML3D Geometry
     * @type {Array}
     * @private
     */
    var _onEntityGeometryCreated = [];

    /**
     * List of handlers that are executed when communicator has established a connection to the server.
     * @type {Array}
     * @private
     */
    var _onConnectionEstablished = [];


    FIVES.Events.AddConnectionEstablishedHandler = function(handler) {
        _onConnectionEstablished.push(handler);
    };

    FIVES.Events.RemoveConnectionEstablishedHandler = function(handler) {
        if(_onConnectionEstablished.indexOf(handler) != -1)
            _onConnectionEstablished.splice(handler, 1);
    };

    FIVES.Events.AddOnComponentUpdatedHandler = function(handler) {
        _onComponentUpdatedHandler.push(handler);
    };

    FIVES.Events.RemoveOnComponentUpdatedHandler = function(handler) {
        if(_onComponentUpdatedHandler.indexOf(handler) != -1)
        _onComponentUpdatedHandler.splice(handler, 1);
    };

    FIVES.Events.AddEntityAddedHandler = function(handler) {
        _onEntityAddedHandler.push(handler);
    };

    FIVES.Events.RemoveEntityAddedHandler = function(handler) {
        if(_onEntityAddedHandler.indexOf(handler) != -1)
            _onEntityAddedHandler.splice(handler, 1);
    };

    FIVES.Events.AddEntityGeometryCreatedHandler = function(handler) {
        _onEntityGeometryCreated.push(handler);
    };

    FIVES.Events.RemoveEntityGeometryCreatedHandler = function(handler) {
        if(_onEntityGeometryCreated.indexOf(handler) != -1)
            _onEntityGeometryCreated.splice(handler, 1);
    };

    /**
     * Fired when FiVES Communicator is done establishing the connection to the server
     * @constructor
     */
    FIVES.Events.ConnectionEstablished = function() {
        for(var i in _onConnectionEstablished) {
            _onConnectionEstablished[i]();
        }
    };


    /**
     * Fired when an entity updated one of its components. May be used by plugin scripts to executed
     * @param entity [FIVES.Models.Entity] Entity object that fired the event
     * @param componentName Name of the component that was updated
     */
    FIVES.Events.ComponentUpdated = function(entity, componentName,attributeName) {
        for(var i in _onComponentUpdatedHandler) {
            _onComponentUpdatedHandler[i](entity, componentName,attributeName);
        }
    };

    /**
     * Fired when a new entity was added to its scene. May be used to create resources based on specific plugins
     * or register events like componentChanged on the entity
     * @param entity
     * @constructor
     */
    FIVES.Events.EntityAdded = function(entity) {
        for(var i in _onEntityAddedHandler) {
            _onEntityAddedHandler[i](entity);
        }
    };

    /**
     * Fired when XML3D geometry was created for an entity. May be used by plugins that need to initialize resources
     * that depend on geometry definitions
     * @param entity
     * @constructor
     */
    FIVES.Events.EntityGeometryCreated = function(entity) {
        for(var i in _onEntityGeometryCreated) {
            _onEntityGeometryCreated[i](entity);
        }
    };

}());
