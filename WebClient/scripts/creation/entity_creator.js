/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 9/18/13
 * Time: 12:40 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Creation = FIVES.Creation || {};

(function () {
    "use strict";

    var EntityCreator = function () {
    };

    var ec = EntityCreator.prototype;

    ec.createEntityFromForm = function() {
        var position = _retrievePosition();
        var orientation = _retrieveOrientation();
        var scale = _retrieveScale();
        var mesh = _retrieveMesh();
        var call = FIVES.Plugins.Editing.createMeshEntity(position, orientation, scale, mesh);
        call.on("result", function(newGuid) {
            console.log("Created Entity with Guid " + newGuid);
        });
    };

    ec.createRandomEntities = function(amount) {
        var i = 0;
        while(i < amount) {
            var position = { x: Math.random() * 10, y: Math.random() * 10, z: Math.random() * 10};
            var scaleFactor = Math.random() * 2 + 0.5;
            var scale = { x: scaleFactor, y: scaleFactor, z: scaleFactor};
            var orientation = { x: 0, y: 0, z: 0, w: 1};
            var mesh = {uri: "resources/models/firetruck/xml3d/firetruck.xml", visible: true};
            var call = FIVES.Plugins.Editing.createMeshEntity(position, orientation, scale, mesh);
            i ++;
        }
    };

    var _retrievePosition = function () {
        var posX = _getValidFloatFieldValue("input-position-x");
        var posY = _getValidFloatFieldValue("input-position-y");
        var posZ = _getValidFloatFieldValue("input-position-z");

        return {x: parseFloat(posX), y: parseFloat(posY), z: parseFloat(posZ)};
    }

    var _retrieveOrientation = function () {
        var rotX = _getValidFloatFieldValue("input-orientation-x");
        var rotY = _getValidFloatFieldValue("input-orientation-y");
        var rotZ = _getValidFloatFieldValue("input-orientation-z");
        var rotW = _getValidFloatFieldValue("input-orientation-w");

        var orientation = new XML3DRotation();
        orientation.setAxisAngle(new XML3DVec3(parseFloat(rotX), parseFloat(rotY), parseFloat(rotZ)), parseFloat(rotW));

        var quat = orientation.getQuaternion();
        return {x: quat[0], y: quat[1], z: quat[2], w: quat [3]};
    }

    var _retrieveScale = function () {
        var scaleX = _getValidFloatFieldValue("input-scale-x");
        var scaleY = _getValidFloatFieldValue("input-scale-y");
        var scaleZ = _getValidFloatFieldValue("input-scale-z");

        return {x: parseFloat(scaleX), y: parseFloat(scaleY), z: parseFloat(scaleZ)};
    }
    var _getValidFloatFieldValue = function (fieldName) {
        var field = $("#" + fieldName);
        if (!field) {
            console.error("[ERROR] EntityCreator._getValidFloatFieldValue: Could not access field with fieldname " + fieldName);
            return;
        }
        var value = field.val();
        if (!value) {
            console.warn("[WARNING] EntityCreator._getValidFloatFieldValue: No value specified for " + fieldName + ", will use defaultvalue instead");
            value = fieldName.indexOf("scale") < 0  ? 0 : 1;
        }
        return value;
    };

    var _retrieveMesh = function () {
        var uri = $("#select-mesh").val();
        return {uri: uri, visible: true};
    }
    FIVES.Creation.EntityCreator = new EntityCreator();
}());