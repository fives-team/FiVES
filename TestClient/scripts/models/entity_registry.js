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
        newEntity.xml3dView = {};

        FIVES.Resources.SceneManager.addMeshForObject(newEntity);
    };

    er.getEntity = function (guid) {
        return this._entities[guid];
    }

    er.rotateAllEntities = function () {
        for (var i in this._entities) {
            var entity = this._entities[i];
            var o = entity.orientation;
            var rotation = new XML3DRotation();
            rotation._setQuaternion([o.x, o.y, o.z, o.w]);
            rotation.angle += 0.1;
            if(rotation.angle > 2* Math.PI)
                rotation.angle = 0;

            var q = rotation.getQuaternion();
            entity.setOrientation(q[0], q[1], q[2], q[3]);
        }
    }

    FIVES.Models.EntityRegistry = new EntityRegistry();
}());
