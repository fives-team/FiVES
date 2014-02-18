/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/14/14
 * Time: 12:18 PM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

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
