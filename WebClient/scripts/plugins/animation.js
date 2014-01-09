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

    var registeredEntities = {};

    function updateLoop() {
        for (var entityGuid in registeredEntities) {
            var entity = FIVES.Models.EntityRegistry._entities[entityGuid];
            for(var animation in registeredEntities[entityGuid])
            {
                entity.increaseAnimationKey(registeredEntities[entityGuid][animation], fps);
            }
        }
    }

    a.startAnimationPlayback = function(entityGuid, animationName, startFrame, endFrame, cycles, speed)
    {
        registeredEntities[entityGuid] = registeredEntities[entityGuid] || [];
        var registeredEntityAnimations = registeredEntities[entityGuid];
        if(registeredEntityAnimations.indexOf(animationName) < 0 )
            registeredEntityAnimations.push(animationName);
    };

    a.stopAnimationPlayback = function(entityGuid, animationName) {
        registeredEntities[entityGuid] = registeredEntities[entityGuid] || [];
        var registeredEntityAnimations = registeredEntities[entityGuid];
        var indexInRunningAnimations = registeredEntityAnimations.indexOf(animationName);
        if(indexInRunningAnimations != -1)
            registeredEntityAnimations.splice(indexInRunningAnimations, 1);
    };


    FIVES.Plugins.Animation = new animation();
}());
