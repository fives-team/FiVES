/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/19/14
 * Time: 9:23 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function () {
    "use strict";

    var _xml3dElement;

    var avatarCollision = function () {
        _xml3dElement = $("xml3d")[0];
    };

    var a = avatarCollision.prototype;

    a.getHitpointWithGround = function(entity) {
        var rayOrigin = getCollisionRayOrigin(entity);
        var ray = new XML3DRay(rayOrigin, new XML3DVec3(0,-1,0));
        var outHitpoint = new XML3DVec3(0,0,0);
        _xml3dElement.getElementByRay(ray, outHitpoint);
        return outHitpoint;
    };

    var getCollisionRayOrigin = function(entity) {
        var boundingBox = entity.xml3dView.groupElement.getBoundingBox();
        var center = boundingBox.center();
        var view = $(_xml3dElement.activeView)[0];
        var viewDirection = view.getDirection();
        var rayOrigin = new XML3DVec3(center.x + viewDirection.x * 0.5, boundingBox.max.y, center.z + viewDirection.z * 0.5);
        return rayOrigin;
    };

}());

