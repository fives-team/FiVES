/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/17/14
 * Time: 2:36 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function() {
    "use strict";

    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var motion = function() {
        _fivesCommunicator.registerFunctionWrapper(this._createFunctionWrappers.bind(this));
    };

    var m = motion.prototype;

    m._createFunctionWrappers = function() {
        this.updateMotion = _fivesCommunicator.connection.generateFuncWrapper("motion.update");
    }

    FIVES.Plugins.Motion = new motion();
}());
