/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 9/18/13
 * Time: 9:15 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Models = FIVES.Models || {};

(function (){
     "use strict";

    var Entity = function(entityDocument) {
        this.guid = entityDocument.guid;
        this.position = entityDocument.position;
        this.orientation = entityDocument.orientation;
        this.scale= entityDocument.scale;
        this.meshResource = entityDocument.meshResource;

    };

    var e = Entity.prototype;

    e.retrieveEntityDataFromServer = function() {
        this.retrieveLocationFromServer();
        this.retrieveMeshDataFromServer();
    } ;

    e.retrieveLocationFromServer = function () {
        FIVES.Communication.FivesCommunicator.getObjectLocation(this.guid).on("result", _handleLocationUpdate.bind(this));
    };

    var _handleLocationUpdate = function(error, location) {
        this.location = this.location || {};
        this.location.position = location.position;
        this.location.orientation = location.orientation;
    };

    e.retrieveMeshDataFromServer = function () {
        FIVES.Communication.FivesCommunicator.getObjectMesh(this.guid).on("result", _handleMeshUpdate.bind(this));
    };

    var _handleMeshUpdate = function(error, mesh) {
        this.mesh = this.mesh || {};
        this.mesh.scale = mesh.scale;
        this.mesh.scale.x = 1;
        this.mesh.scale.y = 1;
        this.mesh.scale.z = 1;

        this.mesh.uri = mesh.uri || "resources/models/firetruck/xml3d/firetruck.xml";
        FIVES.Resources.SceneManager.addMeshForObject(this);
    };

    e.setOrientation = function(x, y, z, w) {
        var newOrientation = { x: x, y: y, z: z, w: w};
        this.location.orientation = newOrientation;
        FIVES.SceneManager.updateOrientation(this);
    };

    FIVES.Models.Entity = Entity;
}());
