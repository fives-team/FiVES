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
FIVES.Plugins = FIVES.Plugins || {};

(function (){
    "use strict";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var location = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
        FIVES.Events.AddOnComponentUpdatedHandler(this._componentUpdatedHandler.bind(this));
    };

    var l = location.prototype;

    l._createFunctionWrappers = function (){
        this.updateEntityPosition = _fivesCommunicator.connection.generateFuncWrapper("location.updatePosition");
        this.updateEntityOrientation = _fivesCommunicator.connection.generateFuncWrapper("location.updateOrientation");
    };

    l._componentUpdatedHandler = function(entity, componentName, attributeName) {
        if(componentName == "location")
        {
                FIVES.Resources.SceneManager.applyPositionToXML3DView(entity);
                FIVES.Resources.SceneManager.applyOrientationToXML3DView(entity);
        }
    };

    l.updatePosition = function(entity, position) {
        entity.location.position = position;
        FIVES.Resources.SceneManager.applyPositionToXML3DView(entity);
    };

    l.updateOrientation = function(entity, orientation) {
        entity.location.orientation = orientation;
        FIVES.Resources.SceneManager.applyOrientationToXML3DView(entity);
    };

    l.setEntityPosition = function(entity, x, y, z) {
        this.sendEntityPositionUpdate(entity.guid, {x: x, y: y, z: z});
    };

    l.setEntityOrientation = function(entity, x, y, z, w) {
        this.sendEntityOrientationUpdate(entity.guid, { x: x, y: y, z: z, w: w});
    };

    l.sendEntityPositionUpdate = function(guid, position) {
        this.updateEntityPosition(guid, position, _fivesCommunicator.generateTimestamp());
    };

    l.sendEntityOrientationUpdate = function(guid, orientation) {
        this.updateEntityOrientation(guid, orientation, _fivesCommunicator.generateTimestamp());
    };

    FIVES.Plugins.Location = new location();

}());
