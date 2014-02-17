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
        // meshResouce.meshURI should actually be assigned to mesh.uri.
        // All uses of these properties must be updated respectively.
        this.xml3dView = {};
        this.guid = entityDocument.guid;
        this.location = entityDocument.location
            || {position: {x:0,y:0,z:0}, orientation: {x:0,y:0,z:0,w:1}};

        this.mesh = entityDocument.mesh ||
            {uri:"",visible:true, scale: {x:1, y:1, z:1 }};

        this._cachedComponentUpdates = {};
        this._attributeUpdateHandle = setInterval(this._flushUpdates.bind(this), 30);
    };

    var e = Entity.prototype;

    e._flushUpdates = function() {
        for(var updatedComponent in this._cachedComponentUpdates) {
            this._applyAttributeUpdates(updatedComponent);
        };

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
            this._applyComponentUpdatesTo3DView(componentName, updatedAttribute);
    };

    e._applyComponentUpdatesTo3DView = function(componentName, attributeName) {
        FIVES.Events.ComponentUpdated(this, componentName,attributeName);
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
