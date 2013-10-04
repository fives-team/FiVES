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

    e.updatePosition = function(position) {
        this.position = position;
        FIVES.Resources.SceneManager.updatePosition(this);
    };

    e.updateOrientation = function(orientation) {
        this.orientation = orientation;
        FIVES.Resources.SceneManager.updateOrientation(this);
    };

    e.setPosition = function(x, y, z) {
        this.position = {x: x, y: y, z: z};
        FIVES.Resources.SceneManager.updatePosition(this);
        FIVES.Communication.FivesCommunicator.sendEntityPositionUpdate(this.guid, this.position);
    };

    e.setOrientation = function(x, y, z, w) {
        this.orientation = { x: x, y: y, z: z, w: w};
        FIVES.Resources.SceneManager.updateOrientation(this);
        FIVES.Communication.FivesCommunicator.sendEntityOrientationUpdate(this.guid, this.orientation );
    };

    e.getTransformElement = function() {
        if(!this.xml3dView.transformElement)
        {
           //  console.warn("[WARNING] No transform element found for entity " + this.guid);
            return false;
        }
        else
        {
            return this.xml3dView.transformElement;
        }
    };

    FIVES.Models.Entity = Entity;
}());
