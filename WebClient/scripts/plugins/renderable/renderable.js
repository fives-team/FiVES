/**
 * Created with JetBrains WebStorm.
 * Author: Torsten Spieldenner
 * Date: 2/17/14
 * Time: 9:41 AM
 * (c) DFKI 2013
 * http://www.dfki.de
 */

var FIVES = FIVES || {};
FIVES.Plugins = FIVES.Plugins || {};

(function () {
    "use strict";

    var _xml3dElement =  FIVES.Resources.SceneManager.xml3dElement;

    var renderable = function () {
        FIVES.Events.AddEntityAddedHandler(this.addMeshForEntity.bind(this));
        FIVES.Events.AddOnComponentUpdatedHandler(this.updateMesh.bind(this));
    };

    var r = renderable.prototype;

    r.addMeshForEntity = function(entity) {
        if(entity.meshResource.uri)
            FIVES.Resources.ResourceManager.loadExternalResource(entity, this._addMeshToScene.bind(this));
    };

    r._addMeshToScene = function(meshDocument, entityGuid) {
        var entity = FIVES.Models.EntityRegistry.getEntity(entityGuid);
        this._addMeshDefinitionsToScene(entity, meshDocument);
        this._addXml3dGroupsForMesh(entity, meshDocument);
    };

    r._addMeshDefinitionsToScene = function(entity, meshDocument) {
        var meshDefinitions = $(meshDocument).children("defs");
        $(_xml3dElement).append(meshDefinitions);
        entity.xml3dView.defElement = meshDefinitions[0];
    };

    r._addXml3dGroupsForMesh = function(entity, meshDocument) {
        var meshGroup = $(meshDocument).children("group");
        var entityGroup = this._createParentGroupForEntity(entity);
        entity.xml3dView.groupElement = entityGroup;
        entity.xml3dView.groupElement.setAttribute("visible", entity["meshResource"]["visible"]);
        _xml3dElement.appendChild(entity.xml3dView.groupElement);
        $(entity.xml3dView.groupElement).append(meshGroup);
    };

    r._createParentGroupForEntity = function(entity) {
        var entityGroup = XML3D.createElement("group");
        entityGroup.setAttribute("id", "Entity-" + entity.guid);
        entityGroup.setAttribute("transform", "#transform-" + entity.guid );
        return entityGroup;
    };

    r.updateMesh = function(entity) {
        // FIXME: We do not support changes to mesh resource attribute. The correct approach would be to remove existing entity
        // (group and transform elements) from the scene graph and re-create them with the new URI. However removing the group
        // element, which has a reference to the data element within this group, causes crashes in current implement of xml3d.js
        // ([put the url to respective issue on the xml3d issue tracker here]). Once this is fixed, we should use the following
        // code instead:
        //scm.removeEntity(entity);
        //scm.addMeshForEntity(entity).
        // The issue is filed in the xml3d github repo (#50)
        entity.xml3dView.groupElement.setAttribute("visible", entity["meshResource"]["visible"]);
    };

    FIVES.Plugins.Renderable = new renderable();

}());

