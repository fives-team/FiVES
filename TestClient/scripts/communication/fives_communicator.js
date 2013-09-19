/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 9/18/13
 * Time: 9:22 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Communication = FIVES.Communication || {};

( function(){
     "use strict";

    var FivesCommunicator = function() {};

    var connection;

    var c = FivesCommunicator.prototype;

    // Function wrappers for KIARA interface provided by FIVES server
    c.listObjects = function() {};
    c.getObjectLocation = function() {};
    c.createEntityAt = function() {};
    c.createServerScriptFor = function() {};
    c.notifyAboutNewObjects = function() {};
    c.getObjectMesh = function() {};

    c.initialize = function(context, service) {
        this.context = context;
        context.openConnection(service, _onOpenedConnection.bind(this) );
    };

    var _onOpenedConnection = function(error, conn) {
        connection = conn;
        var implementsServices = connection.generateFuncWrapper("kiara.implements");
        implementsServices(["objectsync"]).on("result", _createFunctionWrappers.bind(this));
    };

    var _listObjectsCallback =  function(error, objects) {
        for (var i = 0; i < objects.length; i++)
            FIVES.Models.EntityRegistry.addEntityFromServer(objects[i]);
    };

    var _createFunctionWrappers = function(error, supported) {
        if (supported[0]) {
            this.listObjects = connection.generateFuncWrapper("objectsync.listObjects");
            this.getObjectLocation = connection.generateFuncWrapper("objectsync.getObjectLocation");
            this.createEntityAt = connection.generateFuncWrapper("editing.createEntityAt");
            this.createServerScriptFor = connection.generateFuncWrapper("scripting.createServerScriptFor");
            this.notifyAboutNewObjects = connection.generateFuncWrapper("objectsync.notifyAboutNewObjects");
            this.getObjectMesh = connection.generateFuncWrapper("objectsync.getObjectMesh");
            this.notifyAboutNewObjects(FIVES.Models.EntityRegistry.addEntityFromServer);
            this.listObjects().on("result", _listObjectsCallback.bind(this));
        }
    };

    // Expose Communicator to namespace
    FIVES.Communication.FivesCommunicator = new FivesCommunicator();

}());