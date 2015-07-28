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
FIVES.Plugins = FIVES.Plugins || {};

(function () {
    "use strict";

    var _xml3dElement;
    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    /**
     * Avatar Collision Plugin is used to determine if the user runs into some obstacle or has elevated from the ground
     * when exploring the scene. There are two collisions to be checked: First, collision with ground, to put
     * avatar back there if it e.g. moves up or down a slope or step. Second: Collision with geometry in move
     * direction to ensure avatar does not run through walls.
     */
    var avatarCollision = function () {
        _xml3dElement = $("xml3d")[0];
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
        FIVES.Events.AddOnComponentUpdatedHandler(this._handleEntityPositionUpdate.bind(this));
    };

    var a = avatarCollision.prototype;

    a._createFunctionWrappers = function() {
        this.setAvatarGroundlevel = _fivesCommunicator.connection.generateFuncWrapper("avatarCollision.setGroundlevel");
    };

    a._handleEntityPositionUpdate = function(entity) {
        if(entity.guid == FIVES.AvatarEntityGuid)
        {
            this.putMeshOnGround(entity);
            if(entity.motion && entity.motion.velocity)
                this.stopMotionOnCollision(entity);
        }
    };

    /**
     * Casts a ray down from upper bound of avatar towards the ground to determine the height of current ground level.
     * Transmits this value to the server which will then adapt the position of an entity on every position change
     * to take into account changing groundlevels
     * @param entity The Entity that shall be put on the ground
     */
    a.putMeshOnGround = function(entity) {
        var hitpointWithGround = this._getHitpointWithGround(entity);
        if(hitpointWithGround && !isNaN(hitpointWithGround.y))
        {
            this.setAvatarGroundlevel(FIVES.AvatarEntityGuid, hitpointWithGround.y);
        }
    };

    a._getHitpointWithGround = function(entity) {
        var rayOrigin = getCollisionRayOrigin(entity);
        var ray = new XML3DRay(rayOrigin, new XML3DVec3(0,-1,0));
        var outHitpoint = new XML3DVec3(0,0,0);
        _xml3dElement.getElementByRay(ray, outHitpoint);
        return outHitpoint;
    };

    /**
     * Casts a ray in movement direction and stops the avatar motion if it hits an obstacle
     * @param entity The Entity for which collision check shall be performed
     */
    a.stopMotionOnCollision = function(entity) {
        var outHitpoint = this._getHitpointInMovementDirection(entity);
        if(outHitpoint.x && !isNaN(outHitpoint.x)
            && outHitpoint.z && !isNaN(outHitpoint.z))
        {
            var entityPositionInPlane = new XML3DVec3(entity.location.position.x, 0, entity.location.position.z);
            var hitpointInPlane = new XML3DVec3(outHitpoint.x, 0, outHitpoint.z);
            if(entityPositionInPlane.subtract(hitpointInPlane).length() < 2.5)
            {
                FIVES.Plugins.Avatar.setAvatarForwardBackwardMotion(0);
            }
        }
    };

    a._getHitpointInMovementDirection = function(entity) {
        var rayOrigin = getCollisionRayOrigin(entity, "forward");
        var view = $(_xml3dElement.activeView)[0];
        var entityDirection = entity.xml3dView.transformElement.rotation.rotateVec3(new XML3DVec3(1,0,0));
        if(entity.motion.velocity.x < 0)
            entityDirection = entityDirection.negate();

        var ray = new XML3DRay(rayOrigin, entityDirection);

        var outHitpoint = new XML3DVec3(0,0,0);
        _xml3dElement.getElementByRay(ray, outHitpoint);
        return outHitpoint;
    };

    var getCollisionRayOrigin = function(entity, collisionDirection) {
        var boundingBox = entity.xml3dView.groupElement.getBoundingBox();
        var center = boundingBox.center();
        var view = $(_xml3dElement.activeView)[0];
        var viewDirection = view.getDirection();
        var rayOrigin;
        if(collisionDirection == "forward")
            rayOrigin = new XML3DVec3(center.x + viewDirection.x * 0.5,
                boundingBox.center().y,
                center.z + viewDirection.z * 0.5);
        else
            rayOrigin = new XML3DVec3(center.x + viewDirection.x * 0.5,
                boundingBox.max.y,
                center.z + viewDirection.z * 0.5);

        return rayOrigin;
    };

    FIVES.Plugins.AvatarCollision = new avatarCollision();
}());

