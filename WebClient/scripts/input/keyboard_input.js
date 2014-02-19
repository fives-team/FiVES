/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 10/18/13
 * Time: 1:20 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Input = FIVES.Input || {};

(function() {
     "use strict";

    var MOVE_SPEED = 0.1;
    var SPIN_SPEED = 0.05;

    var UP_AXIS = {x: 0, y: 1, z: 0};

    var keylistener = function(){
        FIVES.Events.AddEntityGeometryCreatedHandler(this._handleGeometryLoaded.bind(this));
    };

    var k = keylistener.prototype;

    var _pressedKeys = {};
    var _upKeys = {};

    k._handleGeometryLoaded = function(entity) {
        if(entity.guid == FIVES.AvatarEntityGuid)
            this._initializeEventListeners();
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
                //FIVES.Communication.FivesCommunicator.startServersideAnimation(FIVES.AvatarEntityGuid, "walk", e.xml3dView.xflowAnimations.walk.startKey, e.xml3dView.xflowAnimations.walk.endKey);
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
        if(FIVES.Models.EntityRegistry.getEntity(FIVES.AvatarEntityGuid))
        {
            if(!(_pressedKeys[87] || _pressedKeys[83] || _pressedKeys[65] || _pressedKeys[68]))
                // FIVES.Communication.FivesCommunicator.stopServersideAnimation(FIVES.AvatarEntityGuid, "walk");
                FIVES.Plugins.Animation.stopClientsideAnimation(FIVES.AvatarEntityGuid, "walk");
        }
    };

    k._initializeEventListeners = function() {
        $(document).keydown(this._onKeyDown);
        $(document).keyup(this._onKeyUp);
    };

    FIVES.Input.KeyListener = new keylistener();

}());

