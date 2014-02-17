/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/17/14
 * Time: 9:41 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function () {
    "use strict";

    var renderable = function () {};

    var r = renderable.prototype;

    FIVES.Plugins.Renderable = new renderable();

}());

