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

    a.increaseAnimationKeys = function(entity, frameDuration) {
        var playingAnimations = entity.playingAnimationsCollection;
        if(playingAnimations)
        {
            for(var animationName in playingAnimations)
            {
                var playingAnimation = playingAnimations[animationName];
                var xflowKey = entity.xml3dView.xflowAnimations[animationName].key;

                var newValue = this._computeNewKeyframeValue(playingAnimation, frameDuration);
                playingAnimation.currentFrame = newValue;
                xflowKey.text(newValue);
            }
        }
    };

    a._computeNewKeyframeValue = function(playingAnimation, frameDuration) {
        // Animation speed is designed such that a speed of 1 means 1 second of animation per keyframe.
        // We compute the new value based on last frame's duration so that we increase keys higher when the
        // rendering the last frame took long
        var newValue = playingAnimation.currentFrame + playingAnimation.speed * (frameDuration / 1000.0);
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
