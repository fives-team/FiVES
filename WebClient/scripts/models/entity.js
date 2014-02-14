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
        // FIXME: this should assign values to correct attributes, e.g.
        // meshResouce.meshURI should actually be assigned to meshResource.uri.
        // All uses of these properties must be updated respectively.
        this.guid = entityDocument.guid;
        this.position = entityDocument.position || {x:0,y:0,z:0};
        this.orientation = entityDocument.orientation || {x:0,y:0,z:0,w:1};
        this.scale= entityDocument.scale|| {x:1,y:1,z:1};
        this.meshResource = entityDocument.meshResource || {uri:"",visible:true};
        this._cachedComponentUpdates = {};
        this._attributeUpdateHandle = setInterval(this._flushUpdates.bind(this), 30);
    };

    var e = Entity.prototype;

    e._flushUpdates = function() {
        for(var updatedComponent in this._cachedComponentUpdates) {
            this._applyAttributeUpdates(updatedComponent);
        };

        if(this.guid == FIVES.AvatarEntityGuid)  {
            FIVES.Resources.SceneManager.updateCameraView(this);
        }

        this._cachedComponentUpdates = {};
    };

    e._applyAttributeUpdates = function(componentName) {
        var updatedComponent = this._cachedComponentUpdates[componentName];
        this[componentName] = this[componentName] || {};
        for(var updatedAttribute in updatedComponent)
        {
            this[componentName][updatedAttribute] = updatedComponent[updatedAttribute];
        }

        if(this.xml3dView.groupElement && this.xml3dView.transformElement)
            this._applyComponentUpdatesTo3DView(componentName);
    };

    e._applyComponentUpdatesTo3DView = function(componentName) {
        if (componentName == "meshResource")
            FIVES.Resources.SceneManager.updateMesh(this);
        FIVES.Events.ComponentUpdated(this, componentName);
    };

    e.updateAttribute = function(componentName, attributeName, value) {
        this._cachedComponentUpdates[componentName] = this._cachedComponentUpdates[componentName] || {};
        this._cachedComponentUpdates[componentName][attributeName] = value;
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

    e.getDirection = function() {
        var entityTransformation = this.getTransformElement();
        var xAxis = new XML3DVec3(-1,0,0); /* we apply inversve transform */
        return entityTransformation.rotation.rotateVec3(xAxis);
    };

    FIVES.Models.Entity = Entity;
}());
