/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 9/10/13
 * Time: 11:23 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Resources = FIVES.Resources || {};

(function (){
    "use strict";

    var SceneManager = function() {};
    var scm = SceneManager.prototype;
    var _mainDefs;

    scm.initialize = function(xml3dElementId) {
        this._getXml3dElement(xml3dElementId);
        this._createMainDefinitions();
        FIVES.Events.AddEntityAddedHandler(this._addXml3dTranformForMesh.bind(this));
    };

    scm._getXml3dElement = function(xml3dElementId) {
        var _xml3dElement = document.getElementById(xml3dElementId);
        if(!_xml3dElement || _xml3dElement.tagName != "xml3d")
            console.error("[ERROR] (SceneManager) : Cannot find XML3D element with id " + xml3dElementId);
        this.xml3dElement = _xml3dElement;
    };

    scm._createMainDefinitions = function() {
        _mainDefs = XML3D.createElement("defs");
        this.xml3dElement.appendChild(_mainDefs);
    };

    scm.removeEntity = function(entity) {

        if (entity.xml3dView.groupElement) {
            this.xml3dElement.removeChild(entity.xml3dView.groupElement);
            delete entity.xml3dView.groupElement;
        }

        if (entity.xml3dView.transformElement) {
            _mainDefs.removeChild(entity.xml3dView.transformElement);
            delete entity.xml3dView.transformElement;
        }

        if(entity.xml3dView.defElement) {
            this.xml3dElement.removeChild(entity.xml3dView.defElement);
            delete entity.xml3dView.defElement;
        }
    };

    scm._addXml3dTranformForMesh = function(entity) {
        var transformGroup = this._createTransformForEntityGroup(entity);
        entity.xml3dView.transformElement = transformGroup;
    };

    scm._createTransformForEntityGroup = function(entity) {
        var transformTag = XML3D.createElement("transform");
        transformTag.setAttribute("id", "transform-" + entity.guid) ;
        transformTag.translation.set(this._createTranslationForEntityGroup(entity));
        transformTag.rotation.set(this._createRotationFromOrientation(entity));
        transformTag.scale.set(this._createScaleForEntityGroup(entity));
        _mainDefs.appendChild(transformTag);
        return transformTag;
    };

    scm._createTranslationForEntityGroup = function(entity) {
        var position = entity.location.position;
        var xml3dPosition = new XML3DVec3(position.x, position.y, position.z);
        return xml3dPosition;
    };

    scm._createRotationFromOrientation = function(entity) {
        var orientation = entity.location.orientation;
        var axisAngleRotation = new XML3DRotation();
        axisAngleRotation.setQuaternion(orientation, orientation.w);
        return axisAngleRotation;
    };

    scm._createScaleForEntityGroup = function(entity) {
        var scale = entity.mesh.scale;
        var xml3dScale = new XML3DVec3(scale.x, scale.y, scale.z);
        return xml3dScale;
    };

    scm.updateOrientation = function(entity) {
        var transformationForEntity = entity.getTransformElement();
        if(transformationForEntity)
            transformationForEntity.rotation.set(this._createRotationFromOrientation(entity));
    };

    scm.updatePosition = function(entity) {
        var transformationForEntity = entity.getTransformElement();
        if(transformationForEntity)
            transformationForEntity.translation.set(this._createTranslationForEntityGroup(entity));
    };

    /**
     * Puts the active view of the XML3D view behind an entity to follow it in third person view. May for example
     * be used by avatar plugin to position the camera behind the user's avatar
     * @param entity Entity that shall be inspected in 3rd person mode
     */
    scm.setCameraViewToEntity = function(entity) {
        var view = $(this.xml3dElement.activeView)[0];
        var entityTransform = entity.xml3dView.transformElement;
        if(entityTransform)
        {
            view.setDirection(entityTransform.rotation.rotateVec3(new XML3DVec3(1,0,0)));
            var viewDirection = view.getDirection();
            var cameraTranslation = entityTransform.translation.subtract(viewDirection.scale(6));
            cameraTranslation.y = 1.2;
            view.position.set(cameraTranslation);
        }
    };

    FIVES.Resources.SceneManager = new SceneManager();
}());
