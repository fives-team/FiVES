/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/14/14
 * Time: 10:59 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Events = FIVES.Events || {};

(function () {
    "using strict";

    /**
     * List of functions that are executed when an entity fires an attribute changed event
     * @type {Array}
     */
    var _onComponentUpdatedHandler = [];

    FIVES.Events.AddOnComponentUpdatedHandler = function(handler) {
        _onComponentUpdatedHandler.push(handler);
    };

    FIVES.Events.RemoveOnComponentUpdatedHandler = function(handler) {
        if(_onComponentUpdatedHandler.indexOf(handler) != -1)
        _onComponentUpdatedHandler.splice(handler, 1);
    };

    /**
     * Fired when an entity updated one of its components. May be used by plugin scripts to executed
     * @param entity [FIVES.Models.Entity] Entity object that fired the event
     * @param componentName Name of the component that was updated
     */
    FIVES.Events.ComponentUpdated = function(entity, componentName) {
        for(var i in _onComponentUpdatedHandler) {
            _onComponentUpdatedHandler[i](entity, componentName);
        }
    }
}());
