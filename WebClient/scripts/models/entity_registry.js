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
FIVES.Models = FIVES.Models || {};

(function () {
    "use strict";

    var EntityRegistry = function () {
    };

    var er = EntityRegistry.prototype;

    er._entities = {};

    er.addEntityFromServer = function (entityDocument) {
        var newEntity = new FIVES.Models.Entity(entityDocument);
        this._entities[entityDocument.guid] = newEntity;
        FIVES.Events.EntityAdded(newEntity);
    };

    er.removeEntity = function(entityGuid) {
        var entities = FIVES.Models.EntityRegistry._entities;
        FIVES.Resources.SceneManager.removeEntity(entities[entityGuid]);
        delete FIVES.Models.EntityRegistry._entities[entityGuid];
    };

    er.getEntity = function (guid) {
        return this._entities[guid];
    };

    FIVES.Models.EntityRegistry = new EntityRegistry();
}());
