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
              /* W */  case 87:  FIVES.Communication.FivesCommunicator.setAvatarForwardBackwardMotion(FIVES.Communication.FivesCommunicator.sessionKey,  0.1); break;
              /* S */  case 83:  FIVES.Communication.FivesCommunicator.setAvatarForwardBackwardMotion(FIVES.Communication.FivesCommunicator.sessionKey,  -0.1); break;
              /* A */  case 65:  FIVES.Communication.FivesCommunicator.setAvatarLeftRightMotion(FIVES.Communication.FivesCommunicator.sessionKey,  -0.1); break;
              /* D */  case 68:  FIVES.Communication.FivesCommunicator.setAvatarLeftRightMotion(FIVES.Communication.FivesCommunicator.sessionKey,  0.1); break;
                default: break;
            }
            _pressedKeys[e.keyCode] = true;
        }
    };

    k._onKeyUp = function(e) {

        switch (e.keyCode)
        {
            /* W, S */
            case 87:
            case 83:  FIVES.Communication.FivesCommunicator.setAvatarForwardBackwardMotion(FIVES.Communication.FivesCommunicator.sessionKey,  0); break;
            /* A, D */
            case 65:
            case 68:  FIVES.Communication.FivesCommunicator.setAvatarLeftRightMotion(FIVES.Communication.FivesCommunicator.sessionKey,  0); break;
        }
        _pressedKeys[e.keyCode] = false;
        console.log("up");
    };

    k._initializeEventListeners = function() {
        document.addEventListener("keydown", this._onKeyDown);
        document.addEventListener("keyup", this._onKeyUp);
    };

    FIVES.Input.KeyListener = new keylistener();

}());
