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
        if(fivesObject.meshResource.uri && fivesObject.meshResource.visible)
            FIVES.Resources.ResourceManager.loadExternalResource(fivesObject, this._addMeshToScene.bind(this));
    };

    scm.removeEntity = function(entity) {
        if (entity.xml3dView.transformElement) {
            _mainDefs.removeChild(entity.xml3dView.transformElement);
            delete entity.xml3dView.transformElement;
        }

        if (entity.xml3dView.groupElement) {
            _xml3dElement.removeChild(entity.xml3dView.groupElement);
            delete entity.xml3dView.groupElement;
        }
    };

    scm._addMeshToScene = function(meshGroup, idSuffix) {
        var entity = FIVES.Models.EntityRegistry.getEntity(idSuffix);
        var transformGroup = this._createTransformForEntityGroup(entity);
        var entityGroup = this._createParentGroupForEntity(entity);
        var animationDefinitons = this._createAnimationsForEntity(meshGroup, idSuffix);
        entity.xml3dView.transformElement = transformGroup;
        entity.xml3dView.groupElement = entityGroup;
        entity.xml3dView.xflowAnimations = animationDefinitons;
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
        transformTag.translation.set(this._createTranslationForEntityGroup(entity));
        transformTag.rotation.set(this._createRotationFromOrientation(entity));
        transformTag.scale.set(this._createScaleForEntityGroup(entity));
        _mainDefs.appendChild(transformTag);
        return transformTag;
    };

    scm._createTranslationForEntityGroup = function(entity) {
        var position = entity.position;
        var xml3dPosition = new XML3DVec3(position.x, position.y, position.z);
        return xml3dPosition;
    };

    scm._createRotationFromOrientation = function(entity) {
        var orientation = entity.orientation;
        var axisAngleRotation = new XML3DRotation();
        axisAngleRotation.setQuaternion(orientation, orientation.w);
        return axisAngleRotation;
    };

    scm._createScaleForEntityGroup = function(entity) {
        var scale = entity.scale;
        var xml3dScale = new XML3DVec3(scale.x, scale.y, scale.z);
        return xml3dScale;
    };

    // Parses the XML3D model file for <anim> tags that define xflow keyframe animations.
    // Within the definition, the id value of the respective xflow key is stated as appearing
    // in the model file, i.e. ignoring adaptions made to id attributes when adding the entity to the scene.
    // We therefore need to take this adaption into account here separately
    scm._createAnimationsForEntity = function(meshGroup, entityId) {
        var animationDefinitions = {};
        var meshAnimations = $(meshGroup).find("anim");
        meshAnimations.each(function(index, element)
            {
                var animationDefinition = scm._parseAnimationEntry(element, entityId);
                animationDefinition.key = $(meshGroup).find(animationDefinition.key +"-"+entityId);
                animationDefinitions[element.getAttribute("name")] = animationDefinition;
            });
        return animationDefinitions;
    };

    scm._parseAnimationEntry = function(animationDefinition,entityId) {
        var animation = {};
        animation.startKey = animationDefinition.getAttribute("startKey");
        animation.endKey = animationDefinition.getAttribute("endKey");
        animation.speed = animationDefinition.getAttribute("speed");
        animation.key = animationDefinition.getAttribute("key");
        return animation;
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

    scm.updateMesh = function(entity) {
        // When mesh URI is updated, we need to download new model and recreate the mesh in the scene again. Also, as
        // visible attribute is not yet supported for <group> nodes in XML3D, we also need to remove/add the entity
        // from/to the scene to correctly handle visible attribute in the meshResource component.
        scm.removeEntity(entity);
        scm.addMeshForObject(entity);
    };

    scm.updateCameraView = function(entity) {
        var view = $(_xml3dElement.activeView)[0];
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
