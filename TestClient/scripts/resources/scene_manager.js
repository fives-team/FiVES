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

    var _xml3dElement;
    var _mainDefs;

    var EntityRegistry = {};

    scm.initialize = function(xml3dElementId) {
        _xml3dElement = document.getElementById(xml3dElementId);
        if(!_xml3dElement || _xml3dElement.tagName != "xml3d")
            console.error("[ERROR] (SceneManager) : Cannot find XML3D element with id " + xml3dElementId);
        _mainDefs = XML3D.createElement("defs");
        _xml3dElement.appendChild(_mainDefs);
    };

    scm.addMeshForObject = function(fivesObject) {
        if(!fivesObject.meshResource.meshURI)
            console.error("[ERROR] (SceneManager).addMeshForObject : No Resource URI specified for object " + fivesObject.guid);
        else
            FIVES.Resources.ResourceManager.loadExternalResource(fivesObject, this._addMeshToScene.bind(this));
    };

    scm._addMeshToScene = function(meshGroup, idSuffix) {
        var entity = FIVES.Models.EntityRegistry.getEntity(idSuffix);
        var transformGroup = this._createTransformForEntityGroup(entity);
        var entityGroup = this._createParentGroupForEntity(entity);
        entity.xml3dView.groupElement = entityGroup;
        _xml3dElement.appendChild(entityGroup);
        entityGroup.appendChild(meshGroup);
    };

    scm._createParentGroupForEntity = function(entity) {
        var entityGroup = XML3D.createElement("group");
        entityGroup.setAttribute("id", "Entity-" + entity.guid);
        entityGroup.setAttribute("transform", "#transform-" + entity.guid );
        return entityGroup;
    };

    scm._createTransformForEntityGroup = function(entity) {
        var transformTag = XML3D.createElement("transform");
        transformTag.setAttribute("id", "transform-" + entity.guid) ;
        transformTag.setAttribute("translation", this._createTranslationForEntityGroup(entity));
        transformTag.setAttribute("rotation", this._createOrientationForEntityGroup(entity));
        transformTag.setAttribute("scale", this._createScaleForEntityGroup(entity));
        _mainDefs.appendChild(transformTag);
        entity.xml3dView.transformElement = transformTag;
    };

    scm._createTranslationForEntityGroup = function(entity) {
        var position = entity.position;
        var translationAttribute = position.x + " " + position.y + " " + position.z;
        return translationAttribute;
    };

    scm._createOrientationForEntityGroup = function(entity) {
        var orientation = entity.orientation;
        var axisAngleRotation = new XML3DRotation();
        axisAngleRotation.setQuaternion(orientation, orientation.w);
        var axis = axisAngleRotation._axis;
        var angle = axisAngleRotation._angle;
        var orientationAttribute =  axis.x + " " + axis.y + " " + axis.z + " " + angle;
        return orientationAttribute;
    };

    scm._createScaleForEntityGroup = function(entity) {
        var scale = entity.scale;
        var scaleAttribute = scale.x + " " + scale.y + " " + scale.z;
        return scaleAttribute;
    };

    scm.updateOrientation = function(entity) {
        var transformationForEntity = $("#transform-" + entity.guid);
        transformationForEntity.attr("rotation", this._createOrientationForEntityGroup(entity));
    }

    FIVES.Resources.SceneManager = new SceneManager();
}());
