// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function() {
    "use strict";

    var LIGHT_TYPES = ["point", "directional", "spot"];
    var light = function () {
        FIVES.Events.AddEntityAddedHandler(this.addMeshForEntity.bind(this));
    };

    var l = light.prototype;

    l.addLightForEntity = function(entity) {

    };

    FIVES.Plugins.Light = new light();
})();
