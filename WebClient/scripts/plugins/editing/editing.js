/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/17/14
 * Time: 12:14 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function () {
    "use strict";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var editing = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
    };

    var e = editing.prototype;

    e._createFunctionWrappers = function () {
        this.createEntityAt = _fivesCommunicator.connection.generateFuncWrapper("editing.createEntityAt");
        this.createMeshEntity = _fivesCommunicator.connection.generateFuncWrapper("editing.createMeshEntity");
    };

    FIVES.Plugins.Editing = new editing();
}());

