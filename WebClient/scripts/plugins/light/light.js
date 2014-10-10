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
        FIVES.Events.AddEntityAddedHandler(this.addLightForEntity.bind(this));
    };

    var l = light.prototype;

    l.addLightForEntity = function(entity) {
    l._addLightShader = function(entity) {
        var shaderElement = $(_createShaderElement(entity));
        _setShaderParameters(shaderElement, entity);
        $(FIVES.Resources.SceneManager.SceneDefinitions).append(shaderElement);
    };

    var _createShaderElement = function(entity)
    {
        var shaderElement = document.createElement("lightshader");
        var lightType = LIGHT_TYPES[entity["light"]["type"]];
        shaderElement.id = "ls-" + entity.guid;
        shaderElement.setAttribute("script", "urn:xml3d:lightshader:" + lightType);
        return shaderElement;
    };

    var _setShaderParameters = function(shaderElement, entity)
    {
        if(entity["light"]["intensity"]) {
            shaderElement.append(_createShaderParameter("intensity", entity["light"]["intensity"]));
        }

        if(entity["light"]["attenuation"]) {
            shaderElement.append(_createShaderParameter("attenuation", entity["light"]["attenuation"]));
        }
    };

    var _createShaderParameter = function(parameterName, value)
    {
        var parameterTag = $(document.createElement("float3"));
        var parameterValue = value.x + " " + value.y + " " + value.z;
        parameterTag.attr("name", parameterName);
        parameterTag.text(parameterValue);
        return parameterTag;
    };


    };

    FIVES.Plugins.Light = new light();
})();
