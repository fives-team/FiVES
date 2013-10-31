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

    var keylistener = function(){
        this._initializeEventListeners();
    };
    var k = keylistener.prototype;

    var _pressedKeys = {};
    var _upKeys = {};

    k._onKeyDown = function(e) {
        if(!_pressedKeys[e.keyCode])
        {
            switch (e.keyCode)
            {
              /* W */  case 87:  FIVES.Communication.FivesCommunicator.startAvatarMotionInDirection(FIVES.Communication.FivesCommunicator.sessionKey, {'x': 0.1, 'y': 0, 'z': 0}); break;
              /* S */  case 83:  FIVES.Communication.FivesCommunicator.startAvatarMotionInDirection(FIVES.Communication.FivesCommunicator.sessionKey, {'x': -0.1, 'y': 0, 'z': 0}); break;
              /* A */  case 65:  FIVES.Communication.FivesCommunicator.startAvatarMotionInDirection(FIVES.Communication.FivesCommunicator.sessionKey, {'x': 0, 'y': 0, 'z': -0.1}); break;
              /* D */  case 68:  FIVES.Communication.FivesCommunicator.startAvatarMotionInDirection(FIVES.Communication.FivesCommunicator.sessionKey, {'x': 0, 'y': 0, 'z': 0.1}); break;
                default: break;
            }
            _pressedKeys[e.keyCode] = true;
        }
    };

    k._onKeyUp = function(e) {
        // TODO: Hack, implement correct function to stop movement
        FIVES.Communication.FivesCommunicator.startAvatarMotionInDirection(FIVES.Communication.FivesCommunicator.sessionKey, {'x': 0, 'y': 0, 'z': 0});
        _pressedKeys[e.keyCode] = false;
        console.log("up");
    };

    k._initializeEventListeners = function() {
        document.addEventListener("keydown", this._onKeyDown);
        document.addEventListener("keyup", this._onKeyUp);
    };

    FIVES.Input.KeyListener = new keylistener();

}());
