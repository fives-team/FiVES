/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/14/14
 * Time: 11:11 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};
FIVES.Plugins.Animation = FIVES.Plugins.Animation || {};

(function() {
    "using strict";

    var animator = function () {};

    var a = animator.prototype;

    a.setAnimationKeys = function(entity) {
        var animationKeyframes = entity["animation"]["animationKeyframes"].split(';');
        for(var k in animationKeyframes) {
            var animationFrame = animationKeyframes[k];
            if(animationFrame.indexOf(':') != -1)
            {
                var animationWithFrame = animationFrame.split(':');
                var xflowAnimation = entity.xml3dView.xflowAnimations[animationWithFrame[0]];
                if(xflowAnimation)
                {
                    xflowAnimation.key.text(parseFloat(animationWithFrame[1]));
                }
            }
        }
    };

    a.increaseAnimationKeys = function(entity, fps) {
        var playingAnimations = entity.playingAnimationsCollection;
        if(playingAnimations)
        {
            for(var animationName in playingAnimations)
            {
                var playingAnimation = playingAnimations[animationName];
                var xflowKey = entity.xml3dView.xflowAnimations[animationName].key;

                var oldValue = parseFloat(xflowKey.text());
                var newValue = this._computeNewKeyframeValue(playingAnimation, oldValue, fps);
                xflowKey.text(newValue);
            }
        }
    };

    a._computeNewKeyframeValue = function(playingAnimation, oldValue, fps) {
        var newValue = oldValue + playingAnimation.speed * fps / 1000.0;
        if (newValue > playingAnimation.endFrame)
        {
            newValue = this._increaseAnimationCycles(playingAnimation, newValue);
        }
        return newValue;
    };

    a._increaseAnimationCycles = function(playingAnimation, newValue) {
        var frameRange = playingAnimation.endFrame - playingAnimation.startFrame;
        playingAnimation.currentCycle += Math.floor(newValue / frameRange);

        var valueInNewCycle = newValue;
        if(playingAnimation.currentCycle > playingAnimation.cycles && playingAnimation.cycles != -1)
        {
            valueInNewCycle = playingAnimation.endFrame;
            FIVES.Plugins.Animation.stopAnimationPlayback(this.guid, playingAnimation.name);
        }
        else
        {
            valueInNewCycle = playingAnimation.startFrame + (newValue - playingAnimation.endFrame) % frameRange;
        }
        return valueInNewCycle;
    };

    FIVES.Plugins.Animation._keyframeAnimator = new animator();
}());
