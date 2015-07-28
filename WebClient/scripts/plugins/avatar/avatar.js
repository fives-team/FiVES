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

(function () {

    "use strict";
    var _fivesCommunicator = FIVES.Communication.FivesCommunicator;

    var avatar = function() {
        FIVES.Events.AddConnectionEstablishedHandler(this._createFunctionWrappers.bind(this));
        FIVES.Events.AddOnComponentUpdatedHandler(this._setCameraBehindAvatar.bind(this));
        FIVES.Events.AddEntityGeometryCreatedHandler(this._setCameraBehindAvatar.bind(this));
    };

    var a = avatar.prototype;

    a._createFunctionWrappers = function (){
        this.getAvatarEntityGuid =
            _fivesCommunicator.connection.generateFuncWrapper("avatar.getAvatarEntityGuid");
        this.startAvatarMotionInDirection =
            _fivesCommunicator.connection.generateFuncWrapper("avatar.startAvatarMotionInDirection");
        this.setAvatarForwardBackwardMotion =
            _fivesCommunicator.connection.generateFuncWrapper("avatar.setAvatarForwardBackwardMotion");
        this.setAvatarLeftRightMotion =
            _fivesCommunicator.connection.generateFuncWrapper("avatar.setAvatarLeftRightMotion");
        this.setAvatarSpinAroundAxis =
            _fivesCommunicator.connection.generateFuncWrapper("avatar.setAvatarSpinAroundAxis");

        var getEntityGuidCall = this.getAvatarEntityGuid();
        getEntityGuidCall.on("success", function(avatarEntityGuid) {
            FIVES.AvatarEntityGuid = avatarEntityGuid;
        });
    };

    a._handleComponentUpdate = function(entity, componentName) {
        if(componentName == "position" || componentName == "orientation") {
            this._setCameraBehindAvatar(entity);
        }
    };

    a._setCameraBehindAvatar = function(entity) {
        if(entity.guid == FIVES.AvatarEntityGuid)  {
            FIVES.Resources.SceneManager.setCameraViewToEntity(entity);
        }
    };

    FIVES.Plugins.Avatar = new avatar();
}());
