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

    var _entities = {};

    var er = EntityRegistry.prototype;

    er.addEntityFromServer = function (guid) {
        var entity = new FIVES.Models.Entity();
        entity.guid = guid;
        entity.retrieveEntityDataFromServer();
        _entities[guid] = entity;
    };

    er.getEntity = function (guid) {
        return _entities[guid];
    }

    er.rotateAllEntities = function () {
        for (var i in _entities) {
            var entity = _entities[i];
            entity.location.orientation.y = 1;
            entity.location.orientation.w = 0.75;
            FIVES.Resources.SceneManager.updateOrientation(entity);
        }
    }
    FIVES.Models.EntityRegistry = new EntityRegistry();
}());
