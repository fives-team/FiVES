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

