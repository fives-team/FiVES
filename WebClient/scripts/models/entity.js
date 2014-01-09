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

        if(componentName == "position")
            FIVES.Resources.SceneManager.updatePosition(this);
        else if(componentName == "orientation")
            FIVES.Resources.SceneManager.updateOrientation(this);
        else if (componentName == "meshResource")
            FIVES.Resources.SceneManager.updateMesh(this);
        else if (componentName == "animation")
            this.setAnimationKey();
    };

    e.setAnimationKey = function() {
        var animationKeyframes = this["animation"]["animationKeyframes"].split(';');
        for(var k in animationKeyframes) {
            var animationFrame = animationKeyframes[k];
            if(animationFrame.indexOf(':') != -1)
            {
                var animationWithFrame = animationFrame.split(':');
                var xflowAnimation = this.xml3dView.xflowAnimations[animationWithFrame[0]];
                if(xflowAnimation)
                {
                    xflowAnimation.key.text(parseFloat(animationWithFrame[1]));
                }
            }
        }
    };

    e.increaseAnimationKeys = function(fps) {
        if(this.animationsPlaying)
        {
            for(var animationName in this.animationsPlaying)
            {
                var playingAnimation = this.animationsPlaying[animationName];
                var xflowKey = this.xml3dView.xflowAnimations[animationName].key;

                var oldValue = parseFloat(xflowKey.text());
                var newValue = this._computeNewKeyframeValue(playingAnimation, oldValue, fps);
                xflowKey.text(newValue);
            }
        }
    };

    e._computeNewKeyframeValue = function(playingAnimation, oldValue, fps) {
        var newValue = oldValue + playingAnimation.speed * fps / 1000.0;
        if (newValue > playingAnimation.endFrame)
        {
            newValue = this._increaseAnimationCycles(playingAnimation, newValue);
        }
        return newValue;
    };

    e._increaseAnimationCycles = function(playingAnimation, newValue) {
        playingAnimation.currentCycle ++;
        var valueInNewCycle = newValue;
        if(playingAnimation.currentCycle > playingAnimation.cycles && playingAnimation.cycles != -1)
        {
            valueInNewCycle = 0
            FIVES.Plugins.Animation.stopAnimationPlayback(this.guid, playingAnimation.name);
        }
        else
        {
            valueInNewCycle = playingAnimation.startFrame + (newValue - playingAnimation.endFrame);
        }
        return valueInNewCycle;
    };

    e.updateAttribute = function(componentName, attributeName, value) {
        this._cachedComponentUpdates[componentName] = this._cachedComponentUpdates[componentName] || {};
        this._cachedComponentUpdates[componentName][attributeName] = value;
    };

    e.updatePosition = function(position) {
        this.position = position;
        FIVES.Resources.SceneManager.updatePosition(this);
    };

    e.updateOrientation = function(orientation) {
        this.orientation = orientation;
        FIVES.Resources.SceneManager.updateOrientation(this);
    };

    e.setPosition = function(x, y, z) {
        FIVES.Communication.FivesCommunicator.sendEntityPositionUpdate(this.guid, {x: x, y: y, z: z});
    };

    e.setOrientation = function(x, y, z, w) {
        FIVES.Communication.FivesCommunicator.sendEntityOrientationUpdate(this.guid, { x: x, y: y, z: z, w: w});
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
