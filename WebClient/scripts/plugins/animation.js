/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 1/8/14
 * Time: 11:33 AM
 * (c) DFKI 2014
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function () {

    "use strict";

    var fps = 30;
    var animation = function () {
        window.setInterval(updateLoop, 1000.0 / fps);
    };

    var a = animation.prototype;

    var registeredEntities = [];

    function updateLoop() {
        for (var i in registeredEntities) {
            var entity = registeredEntities[i];
            if(entity)
                entity.increaseAnimationKeys(fps);
        }
    }

    a.startAnimationPlayback = function(entityGuid, animationName, startFrame, endFrame, cycles, speed)
    {
        var entity = FIVES.Models.EntityRegistry.getEntity[entityGuid];
        if(entity)
        {
            entity.playingAnimationsCollection = entity.playingAnimationsCollection || {};
            entity.playingAnimationsCollection[animationName] = {name: animationName, startFrame: startFrame, endFrame: endFrame, cycles: cycles, currentCycle: 1, speed: speed};

            if(registeredEntities.indexOf(entity) < 0 )
                registeredEntities.push(entity);
        }
    };

    a.stopAnimationPlayback = function(entityGuid, animationName) {
        var entity = FIVES.Models.EntityRegistry.getEntity[entityGuid];

        if(!entity || registeredEntities.indexOf(entity) < 0)
            return;

        if(entity.playingAnimationsCollection && entity.playingAnimationsCollection[animationName])
            delete entity.playingAnimationsCollection[animationName];

        if(Object.keys(entity.playingAnimationsCollection).length == 0)
            registeredEntities.splice(registeredEntities.indexOf(entityGuid), 1);
    };

    FIVES.Plugins.Animation = new animation();
}());
