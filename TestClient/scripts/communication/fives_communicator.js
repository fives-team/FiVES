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

    // Attempts to authenticate. The `callback` is executed as a function with one argument - true if client was
    // authenticated or false if any other error have happened.
    c.auth = function(login, password, callback) {
        var reportFailure = function() {
            callback(false);
        };

        var loginCallback = function(error, result) {
            if (error || result == "") {
                reportFailure();
            } else {
                this.sessionKey = result;
                callback(true);
            }
        };

        var implementsCallback = function(error, result) {
            if (error || !result[0] || !result[1]) {
                reportFailure();
            } else {
                var login = this.connection.generateFuncWrapper("auth.login");
                login(login, password)
                    .on("result", loginCallback)
                    .on("error", reportFailure);
            }
        };

        this.implements(["kiara", "auth"])
            .on("result", implementsCallback)
            .on("error", reportFailure);
    }

    // Attempts to connect to the virtual world. Method `auth` must be used prior to this function to authenticate in
    // the virtual world. The `callback` is executed with one argument - true if the client have been successfully
    // connected or false if some error happened.
    c.connect = function(callback) {
        var requiredServices = ["kiara", "objectsync", "editing", "scripting", "avatar"];
        var reportFailure = function() {
            callback(false);
        };

        var implementsCallback = function(error, result) {
            if (error) {
                reportFailure();
            } else {
                for (var i in result) {
                    if (result[i] === false) {
                        reportFailure();
                        return;
                    }
                }


            }
        };

        this.implements(requiredServices)
            .on("result", implementsCallback)
            .on("error", reportFailure);
    }

    var _onOpenedConnection = function(error, conn) {
        this.connection = conn;
        this.implements = connection.generateFuncWrapper("kiara.implements");
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