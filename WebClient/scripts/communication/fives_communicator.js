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
    var c = FivesCommunicator.prototype;

    c.registeredWrapperRegisterers = [];

    c.initialize = function(context, service) {
        this.context = context;
        context.openConnection(service, _onOpenedConnection.bind(this) );
    };

    // Attempts to authenticate. The `callback` is executed as a function with one argument - true if client was
    // authenticated or false if any other error have happened.
    c.auth = function(username, password, callback) {
        var self = this;

        // If connection has not been established yet - check again in 500 milliseconds.
        if (!self.connection) {
            self.onConnected = c.auth.bind(this, username, password, callback);
            return;
        }

        var reportFailure = function(message) {
            callback(false, message);
        };

        var loginCallback = function(result) {
            if (result == false) {
                reportFailure("Invalid user name or password.");
            } else {
                callback(true);
            }
        };

        var implementsCallback = function(result) {
            if (!result[0] || !result[1]) {
                reportFailure("Server does not support authentication service.");
            } else {
                var login = self.connection.generateFuncWrapper("auth.login");
                login(username, password)
                    .on("success", loginCallback)
                    .on("failure", reportFailure.bind(null, "Failed to authenticate."));
            }
        };

        this.implements(["kiara", "auth"])
            .on("success", implementsCallback)
            .on("failure", reportFailure.bind(null, "Failed to request authentication service from the server."));
    }

    // Attempts to connect to the virtual world. Method `auth` must be used prior to this function to authenticate in
    // the virtual world. The `callback` is executed with one argument - true if the client have been successfully
    // connected or false if some error happened.
    c.connect = function(callback) {
        var self = this;

        var requiredServices = ["kiara", "objectsync", "editing", "avatar"];
        var reportFailure = function(message) {
            callback(false, message);
        };

        var implementsCallback = function(error, result) {
            if (error) {
                reportFailure("Failed to request supported services on the server.");
            } else {
                for (var i in result) {
                    if (result[i] !== true) {
                        reportFailure("Server does not support required service: " + requiredServices[i] + ".");
                        return;
                    }
                }

                _createFunctionWrappers.call(self);
                self.timestampReferenceTime = new Date(2014, 0, 1, 0, 0,0).getTime();
                callback(true);
            }
        };

        this.implements(requiredServices)
            .on("result", implementsCallback)
            .on("error", reportFailure.bind(null, "Failed to request supported services on the server."));
    }

    var _onOpenedConnection = function(error, conn) {
        this.connection = conn;
        this.implements = conn.generateFuncWrapper("kiara.implements");

        if (this.onConnected)
            this.onConnected();
    };

    c.registerFunctionWrapper = function(registerFunction) {
            this.registeredWrapperRegisterers.push(registerFunction);
    };

    c._generateTimestamp = function() {
        var updateTime = new Date().getTime();
        var timeStamp = this.timestampReferenceTime - updateTime;
        return timeStamp;
    };

    var _createFunctionWrappers = function(error, supported) {
        for(var i in this.registeredWrapperRegisterers) {
            this.registeredWrapperRegisterers[i]();
        }
    };

    // Expose Communicator to namespace
    FIVES.Communication.FivesCommunicator = new FivesCommunicator();

}());
