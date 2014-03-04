/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/17/14
 * Time: 12:23 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

define(['kiara', 'websocket-json'], function (KIARA) {
    var testing = function() {
        if (location.hash && location.hash.substr(0, 14) == "#FIVESTesting&")
        {
            var params = {};
            location.hash.substr(14).split("&").forEach(function(item) {
                params[item.split("=")[0]] = item.split("=")[1]
            });

            if (params.OverrideServerPort) {
                var wsjConstructor = KIARA.getProtocol("websocket-json");
                function WSJWithCustomPort(config) {
                    config.port = params.OverrideServerPort;
                    wsjConstructor.call(this, config);
                }
                KIARA.inherits(WSJWithCustomPort, wsjConstructor);
                KIARA.registerProtocol("websocket-json", WSJWithCustomPort);
            }
        }
    };

    var t = testing.prototype;

    FIVES.Plugins.Testing = new testing();

});

