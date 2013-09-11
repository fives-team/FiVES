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

    var EntityRegistry = {};

    scm.initialize = function(xml3dElementId) {
        _xml3dElement = document.getElementById(xml3dElementId);
        if(!_xml3dElement || _xml3dElement.tagName != "xml3d")
            console.error("[ERROR] (SceneManager) : Cannot find XML3D element with id " + xml3dElementId);
    };

    scm.addMeshForObject = function(fivesObject) {
        EntityRegistry[fivesObject.guid] = fivesObject;
        if(!fivesObject.mesh.uri)
            console.error("[ERROR] (SceneManager).addMeshForObject : No Resource URI specified for object " + fivesObject.guid);
        else
            FIVES.Resources.ResourceManager.loadExternalResource(fivesObject, this._addMeshToScene.bind(this));
    };

    scm._addMeshToScene = function(meshGroup, idSuffix) {
        var entityGroup = this._createParentGroupForEntity(EntityRegistry[idSuffix]);
        _xml3dElement.appendChild(entityGroup);
        entityGroup.appendChild(meshGroup);
    };

    scm._createParentGroupForEntity = function(fivesObject) {
        var entityGroup = XML3D.createElement("group");
        entityGroup.setAttribute("id", "Entity-" + fivesObject.guid);
        entityGroup.setAttribute("style", "transform: " + this._createTransformForEntityGroup(fivesObject));
        return entityGroup;
    };

    scm._createTransformForEntityGroup = function(fivesObject) {
        var position = fivesObject.location.position;
        var translationStyle = "translate3d(" + position.x + "px, " + position.y + "px, " + position.z + "px)";
        return translationStyle;
    };

    FIVES.Resources.SceneManager = new SceneManager();
}());
