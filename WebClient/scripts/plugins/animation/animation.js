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

require.config({
    paths: {
        'keyframe_animator' : 'plugins/animation/keyframe_animator'
    }
});

/**
 * ANIMATION PLUGIN
 *
 * Animation plugin is used to employ keyframe animations provided by KeyframeAnimation Plugin of the FiVES server.
 * It registers FunctionWrappers to invoke and stop both server and client side functions.
 * Animations are realized as XFlow-Keyframe animations. Entities will carry objects that encode these animations
 * in their view element, and moreover hold references to the respective Xflow nodes for quick access to the
 * keyframe node without having to perform a DOM lookup.
 */
requirejs(["keyframe_animator"], (function () {

    "use strict";

    var fps = 60;
    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var animation = function () {
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
        FIVES.Events.AddOnComponentUpdatedHandler(this._componentUpdatedHandler.bind(this));
        FIVES.Events.AddEntityGeometryCreatedHandler(this._addXflowAnimationsForMesh.bind(this));

        var boundLoop = updateLoop.bind(this);
        boundLoop();
    };

    var a = animation.prototype;

    /**
     * Callback function that is invoked by the FivesCommunicator after connection to the server was established.
     * Creates function wrappers to start and stop animations and registers to animation update messages from the
     * server.
     */
    a._createFunctionWrappers = function() {
        var conn = _fivesCommunicator.connection;
        this.startServersideAnimation = conn.generateFuncWrapper("animation.startServersideAnimation");
        this.stopServersideAnimation = conn.generateFuncWrapper("animation.stopServersideAnimation");

        this.startClientsideAnimation = conn.generateFuncWrapper("animation.startClientsideAnimation");
        this.stopClientsideAnimation = conn.generateFuncWrapper("animation.stopClientsideAnimation");

        _fivesCommunicator.connection.registerFuncImplementation("animation.receiveClientsideAnimationStart",
            null, this.startAnimationPlayback);

        _fivesCommunicator.connection.registerFuncImplementation("animation.receiveClientsideAnimationStop",
            null, this.stopAnimationPlayback);
    }

    a._componentUpdatedHandler = function(entity, componentName) {
        if(componentName == "animation")
            this._keyframeAnimator.setAnimationKeys(entity);
    };

    /**
     * Entities that are currently registered to have their animation keyframes updated during an update loop.
     * @type {Array}
     */
    var registeredEntities = [];
    var lastFrame;

    function updateLoop(time) {
        // We workaround here for the first computed frame, as we have no value for the last frame yet.
        // We just compute the delta time from the chosen fps parameter then
        var deltaTime = lastFrame ? time - lastFrame : 1000.0 / fps;
        lastFrame = time;
        for (var i in registeredEntities) {
            var entity = registeredEntities[i];
            if(entity)
            {
                this._keyframeAnimator.increaseAnimationKeys(entity, deltaTime);
            }
        }
        var self = this;
        requestAnimationFrame(updateLoop.bind(self));
    };

    a._addXflowAnimationsForMesh = function(entity) {
        var animationDefinitions = this._createAnimationsForEntity(entity);
        entity.xml3dView.xflowAnimations = animationDefinitions;
    };

    // Parses the XML3D model file for <anim> tags that define xflow keyframe animations.
    // Within the definition, the id value of the respective xflow key is stated as appearing
    // in the model file, i.e. ignoring adaptions made to id attributes when adding the entity to the scene.
    // We therefore need to take this adaption into account here separately
    a._createAnimationsForEntity = function(entity) {
        var animationDefinitions = {};
        var $xml3dView = $(entity.xml3dView.groupElement);
        var meshAnimations = $xml3dView.find("anim");
        var that = this;
        meshAnimations.each(function(index, element)
        {
            var animationDefinition = that._parseAnimationEntry(element, entity.guid);
            animationDefinition.key = $xml3dView.find(animationDefinition.key +"-"+entity.guid);
            animationDefinitions[element.getAttribute("name")] = animationDefinition;
        });
        return animationDefinitions;
    };

    a._parseAnimationEntry = function(animationDefinition,entityId) {
        var animation = {};
        animation.startKey = animationDefinition.getAttribute("startKey");
        animation.endKey = animationDefinition.getAttribute("endKey");
        animation.speed = animationDefinition.getAttribute("speed");
        animation.key = animationDefinition.getAttribute("key");
        return animation;
    };

    /**
     * Creates an animation object for an entity that represents the animation that is playing for this entity.
     * Adds the entity to the entities that registered for keyframe updates
     * @param entityGuid Guid of the entity that starts playing the animation
     * @param animationName Name of the animation that shall be played
     * @param startFrame Keyframe from which playback shall start
     * @param endFrame Keyframe at which playback shall stop
     * @param cycles Cycles of animations to be played. -1 denotes infinite playback
     * @param speed Speed at which animation should be played
     */
    a.startAnimationPlayback = function(entityGuid, animationName, startFrame, endFrame, cycles, speed)
    {
        var entity = FIVES.Models.EntityRegistry.getEntity(entityGuid);
        if(entity)
        {
            entity.playingAnimationsCollection = entity.playingAnimationsCollection || {};
            entity.playingAnimationsCollection[animationName] = {
                name: animationName,
                startFrame: startFrame,
                currentFrame: startFrame,
                endFrame: endFrame,
                cycles: cycles,
                currentCycle: 1,
                speed: speed};

            if(registeredEntities.indexOf(entity) < 0 )
            {
                registeredEntities.push(entity);
            }
        }
    };

    /**
     * Stops Playback of an animation for an entity
     * @param entityGuid Guid of the entity for which animation should be stopped
     * @param animationName Name of the animation that should be stopped for the entity
     */
    a.stopAnimationPlayback = function(entityGuid, animationName) {
        var entity = FIVES.Models.EntityRegistry.getEntity(entityGuid);

        if(!entity || registeredEntities.indexOf(entity) < 0)
            return;

        if(entity.playingAnimationsCollection && entity.playingAnimationsCollection[animationName])
            delete entity.playingAnimationsCollection[animationName];

        if(Object.keys(entity.playingAnimationsCollection).length == 0)
        {
            var indexOfEntity = registeredEntities.indexOf(FIVES.Models.EntityRegistry.getEntity(entityGuid));
            registeredEntities.splice(indexOfEntity, 1);
        }
    };

    FIVES.Plugins.Animation = new animation();
}())
);
