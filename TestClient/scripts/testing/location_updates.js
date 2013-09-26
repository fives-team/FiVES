/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 9/26/13
 * Time: 4:29 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Testing = FIVES.Testing || {};

(function() {

    "use strict";

    var LocationUpdates = function () {};

    var l  = LocationUpdates.prototype;
    var er = FIVES.Models.EntityRegistry;

    var movingBackwards = {};

    l.rotateAllEntities = function () {
        for (var i in er._entities) {
            var entity = er._entities[i];
            var o = entity.orientation;
            var rotation = new XML3DRotation();
            rotation._setQuaternion([o.x, o.y, o.z, o.w]);
            rotation.angle += 0.1;
            if(rotation.angle > 2* Math.PI)
                rotation.angle = 0;

            var q = rotation.getQuaternion();
            entity.setOrientation(q[0], q[1], q[2], q[3]);
        }
    };

    l.translateAllEntities = function() {
        for (var i in er._entities) {
            var entity = er._entities[i];
            var p = entity.position;
            var newX = p.x;
            if(movingBackwards[entity.guid])
                newX = p.x - 0.1;
            else
                newX = p.x + 0.1;

            if(newX > 10)
                movingBackwards[entity.guid] = true;
            if(newX < -10)
                movingBackwards[entity.guid] = false;

            entity.setPosition(newX, p.y, p.z);
        }
    };

    l.spinAllEntities = function(delay) {
        setInterval(this.rotateAllEntities.bind(this), delay);
    };

    l.moveAllEntities = function(delay) {
        setInterval(this.translateAllEntities.bind(this), delay);
    };

    FIVES.Testing.LocationUpdates = new LocationUpdates();
}());

