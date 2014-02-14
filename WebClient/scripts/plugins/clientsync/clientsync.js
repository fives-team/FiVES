/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/14/14
 * Time: 1:44 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function(){
    "use script";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var clientsync = function() {
        _fivesCommunicator.registerFunctionWrapper(this._createFunctionWrappers.bind(this));
    };

    var c = clientsync.prototype;

    c._createFunctionWrappers = function () {
        this.listObjects = _fivesCommunicator.connection.generateFuncWrapper("objectsync.listObjects");
        this.notifyAboutNewObjects = _fivesCommunicator.connection.generateFuncWrapper("objectsync.notifyAboutNewObjects");
        this.notifyAboutObjectUpdates = _fivesCommunicator.connection.generateFuncWrapper("objectsync.notifyAboutObjectUpdates");

        this._initUpdateListeners();
    };

    c._initUpdateListeners = function() {
        this.notifyAboutNewObjects(FIVES.Models.EntityRegistry.addEntityFromServer.bind(FIVES.Models.EntityRegistry));
        this.notifyAboutObjectUpdates(this._applyObjectUpdates);
        this.listObjects().on("result", this._listObjectsCallback);
    };

    c._applyObjectUpdates = function(receivedObjectUpdates) {
        for(var entry in receivedObjectUpdates) {
            var handledUpdated = receivedObjectUpdates[entry];
            var updatedEntity = FIVES.Models.EntityRegistry.getEntity(handledUpdated.entityGuid);
            updatedEntity.updateAttribute(handledUpdated.componentName, handledUpdated.attributeName, handledUpdated.value);
        }
    };

    c._listObjectsCallback =  function(error, objects) {
        for (var i = 0; i < objects.length; i++)
            FIVES.Models.EntityRegistry.addEntityFromServer(objects[i]);
    };

    FIVES.Plugins.ClientSync = new clientsync();
}());
