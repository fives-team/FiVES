// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function(){
    "use script";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var clientsync = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
    };

    var c = clientsync.prototype;

    c._createFunctionWrappers = function () {
        _fivesCommunicator.connection.registerFuncImplementation("objectsync.receiveObjectUpdates", null, this._applyObjectUpdates);
        this.listObjects = _fivesCommunicator.connection.generateFuncWrapper("objectsync.listObjects");
        this.notifyAboutNewObjects = _fivesCommunicator.connection.generateFuncWrapper("objectsync.notifyAboutNewObjects");
        this._initUpdateListeners();
    };

    c._initUpdateListeners = function() {
        this.notifyAboutNewObjects(FIVES.Models.EntityRegistry.addEntityFromServer.bind(FIVES.Models.EntityRegistry));
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
