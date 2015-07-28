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

(function (){
     "use strict";

    var Entity = function(entityDocument) {

        this.xml3dView = {};
        for(var componentKey in entityDocument)
        {
            this[componentKey] = entityDocument[componentKey];
        }

        this._cachedComponentUpdates = {};
        this._attributeUpdateHandle = setInterval(this._flushUpdates.bind(this), 30);
    };

    var e = Entity.prototype;

    e._flushUpdates = function() {
        for(var updatedComponent in this._cachedComponentUpdates) {
            this._applyAttributeUpdates(updatedComponent);
        };

        this._cachedComponentUpdates = {};
    };

    e._applyAttributeUpdates = function(componentName) {
        var updatedComponent = this._cachedComponentUpdates[componentName];
        this[componentName] = this[componentName] || {};
        for(var updatedAttribute in updatedComponent)
        {
            this[componentName][updatedAttribute] = updatedComponent[updatedAttribute];
            FIVES.Events.ComponentUpdated(this, componentName,updatedAttribute);
        }
    };

    e.updateAttribute = function(componentName, attributeName, value) {
        this._cachedComponentUpdates[componentName] = this._cachedComponentUpdates[componentName] || {};
        this._cachedComponentUpdates[componentName][attributeName] = value;
    };

    e.getTransformElement = function() {
        if(!this.xml3dView.transformElement)
        {
           //  console.warn("[WARNING] No transform element found for entity " + this.guid);
            return false;
        }
        else
        {
            return this.xml3dView.transformElement;
        }
    };

    e.getDirection = function() {
        var entityTransformation = this.getTransformElement();
        var xAxis = new XML3DVec3(-1,0,0); /* we apply inversve transform */
        return entityTransformation.rotation.rotateVec3(xAxis);
    };

    FIVES.Models.Entity = Entity;
}());
