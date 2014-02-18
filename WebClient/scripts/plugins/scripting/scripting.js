/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/17/14
 * Time: 12:23 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function () {
    "use strict";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var scripting = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
    };

    var s = scripting.prototype;

    s._createFunctionWrappers = function() {
        this.createServerScriptFor =
            _fivesCommunicator.connection.generateFuncWrapper("scripting.createServerScriptFor");
    };

    FIVES.Plugins.Scripting = new scripting();

}());

