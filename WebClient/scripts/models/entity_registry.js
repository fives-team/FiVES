/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 9/18/13
 * Time: 9:33 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Models = FIVES.Models || {};

(function () {
    "use strict";

    var EntityRegistry = function () {
    };

    var er = EntityRegistry.prototype;

    er._entities = {};

    er.addEntityFromServer = function (entityDocument) {
        var newEntity = new FIVES.Models.Entity(entityDocument);
        this._entities[entityDocument.guid] = newEntity;
        FIVES.Events.EntityAdded(newEntity);
    };

    er.getEntity = function (guid) {
        return this._entities[guid];
    };

    FIVES.Models.EntityRegistry = new EntityRegistry();
}());
