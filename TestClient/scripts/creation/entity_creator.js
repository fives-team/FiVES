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
        var mesh = _retrieveMesh();
        var call = FIVES.Communication.FivesCommunicator.createMeshEntity(position, mesh);
        call.on("result", function(newGuid) {
            console.log("Created Entity with Guid " + newGuid);
        });
    };

    ec.createRandomEntities = function(amount) {
        var i = 0;
        while(i < amount) {
            var position = { x: Math.random() * 10, y: Math.random() * 10, z: Math.random() * 10};
            var mesh = {meshUri: "resources/models/firetruck/xml3d/firetruck.xml", visible: true};
            var call = FIVES.Communication.FivesCommunicator.createMeshEntity(position, mesh);
            call.on("result", function(newGuid) {
                console.log("Created Entity with Guid " + newGuid);
            });
            i ++;
        }
    };

    var _retrievePosition = function () {
        var posX = _getValidFloatFieldValue("input-position-x");
        var posY = _getValidFloatFieldValue("input-position-y");
        var posZ = _getValidFloatFieldValue("input-position-z");

        return {x: posX, y: posY, z: posZ};
    }

    var _retrieveOrientation = function () {
        var rotX = _getValidFloatFieldValue("input-position-x");
        var rotY = _getValidFloatFieldValue("input-position-y");
        var rotZ = _getValidFloatFieldValue("input-position-z");
        var rotW = _getValidFloatFieldValue("input-position-w");

        return {x: rotX, y: rotY, z: rotZ, w: rotW};
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
            value = 0;
        }
        return value;
    };

    var _retrieveMesh = function () {
        var meshURI = $("#select-mesh").val();
        return {meshURI: meshURI, visible: true};
    }
    FIVES.Creation.EntityCreator = new EntityCreator();
}());