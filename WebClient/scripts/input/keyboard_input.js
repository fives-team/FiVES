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
FIVES.Input = FIVES.Input || {};

(function() {
    "use strict";

    var MOVE_SPEED = 0.1;
    var SPIN_SPEED = 0.05;

    var UP_AXIS = {x: 0, y: 1, z: 0};

    var avatarEntity;
    var walkAnimation;
    var idleAnimation;

    var keylistener = function(){
        FIVES.Events.AddEntityGeometryCreatedHandler(this._handleGeometryLoaded.bind(this));
    };

    var k = keylistener.prototype;

    var _pressedKeys = {};
    var _upKeys = {};

    k._handleGeometryLoaded = function(entity) {
        if(entity.guid == FIVES.AvatarEntityGuid)
            this._initializeInteractions();
    };

    k._onKeyDown = function(e) {
        if(!_pressedKeys[e.keyCode])
        {
            switch (e.keyCode)
            {
                /* W */ case 87:  FIVES.Plugins.Avatar.setAvatarForwardBackwardMotion(MOVE_SPEED); break;
                /* S */ case 83:  FIVES.Plugins.Avatar.setAvatarForwardBackwardMotion(-MOVE_SPEED); break;
                /* A */ case 65:  FIVES.Plugins.Avatar.setAvatarSpinAroundAxis(UP_AXIS, SPIN_SPEED); break;
                /* D */ case 68:  FIVES.Plugins.Avatar.setAvatarSpinAroundAxis(UP_AXIS, -SPIN_SPEED); break;
                default: break;
            }
            var avatarEntity  = FIVES.Models.EntityRegistry.getEntity(FIVES.AvatarEntityGuid);
            if(avatarEntity)
            {
                if(idleAnimation)
                    FIVES.Plugins.Animation.stopClientsideAnimation(FIVES.AvatarEntityGuid, "idle");
                if(walkAnimation)
                    FIVES.Plugins.Animation.startClientsideAnimation(
                        FIVES.AvatarEntityGuid,
                        "walk",
                        avatarEntity.xml3dView.xflowAnimations.walk.startKey,
                        avatarEntity.xml3dView.xflowAnimations.walk.endKey,
                        -1,
                        1.0);
                _pressedKeys[e.keyCode] = true;
            }
        }
    };

    k._onKeyUp = function(e) {

        switch (e.keyCode)
        {
            /* W, S */
            case 87:
            case 83:  FIVES.Plugins.Avatar.setAvatarForwardBackwardMotion(0); break;
            /* A, D */
            case 65:
            case 68:  FIVES.Plugins.Avatar.setAvatarSpinAroundAxis(UP_AXIS , 0); break;
        }
        _pressedKeys[e.keyCode] = false;

        if(avatarEntity)
        {
            if(!(_pressedKeys[87] || _pressedKeys[83] || _pressedKeys[65] || _pressedKeys[68]))
            {
                if(walkAnimation)
                    FIVES.Plugins.Animation.stopClientsideAnimation(FIVES.AvatarEntityGuid, "walk");
                if(idleAnimation)
                    FIVES.Plugins.Animation.startClientsideAnimation(
                        FIVES.AvatarEntityGuid,
                        "idle",
                        avatarEntity.xml3dView.xflowAnimations.idle.startKey,
                        avatarEntity.xml3dView.xflowAnimations.idle.endKey,
                        -1,
                        1.0);
            }
        }
    };

    k._initializeInteractions = function() {
        $(document).keydown(this._onKeyDown);
        $(document).keyup(this._onKeyUp);

        avatarEntity  = FIVES.Models.EntityRegistry.getEntity(FIVES.AvatarEntityGuid);
        walkAnimation = avatarEntity.xml3dView.xflowAnimations.walk;
        idleAnimation = avatarEntity.xml3dView.xflowAnimations.idle;
    };
    FIVES.Input.KeyListener = new keylistener();

}());

