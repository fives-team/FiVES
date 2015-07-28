// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License version 3
// (LGPL v3) as published by the Free Software Foundation.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU LGPL License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.

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
            var o = entity.location.orientation;
            var rotation = new XML3DRotation();
            rotation._setQuaternion([o.x, o.y, o.z, o.w]);
            rotation.angle += 0.1;
            if(rotation.angle > 2* Math.PI)
                rotation.angle = 0;

            var q = rotation.getQuaternion();
            FIVES.Plugins.Location.setEntityOrientation(entity, q[0], q[1], q[2], q[3]);
        }
    };

    l.translateAllEntities = function() {
        for (var i in er._entities) {
            var entity = er._entities[i];
            var p = entity.location.position;
            var newX = p.x;
            if(movingBackwards[entity.guid])
                newX = p.x - 0.1;
            else
                newX = p.x + 0.1;

            if(newX > 10)
                movingBackwards[entity.guid] = true;
            if(newX < -10)
                movingBackwards[entity.guid] = false;

            FIVES.Plugins.Location.setEntityPosition(entity, newX, p.y, p.z);
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

