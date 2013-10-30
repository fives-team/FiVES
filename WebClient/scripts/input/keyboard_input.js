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

    k._onKeyDown = function(e) {

        if(e.keyCode == 87) { // W

        }

    };

    k._onKeyUp = function(e) {
        console.log(e);
    };

    k._initializeEventListeners = function() {
        document.addEventListener("keydown", this._onKeyDown);
        document.addEventListener("keyup", this._onKeyUp);
    };

    FIVES.Input.KeyListener = new keylistener();

}());
