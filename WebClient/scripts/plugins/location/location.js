/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/14/14
 * Time: 11:55 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function (){
    "use strict";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var location = function() {
        _fivesCommunicator.registerFunctionWrapper(this._createFunctionWrappers.bind(this));
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
