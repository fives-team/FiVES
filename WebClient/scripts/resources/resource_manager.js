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
FIVES.Resources = FIVES.Resources || {};

(function () {
   "use strict";

    var ResourceManager = function() {};
    var rm = ResourceManager.prototype;

    rm.loadExternalResource = function(fivesObject, onResourceLoadedCallback) {
        this._onNextDocumentLoadedCallback = onResourceLoadedCallback;
        CARAMEL.Utility.RemoteXML3DLoader.loadXML3D(fivesObject, this._onLoadedDocument.bind(this));
    };

    rm._onLoadedDocument = function(responseDocument, idSuffix) {
        var adaptedDoc = this._adaptIdAttributes(responseDocument, idSuffix)[0];
        this._onNextDocumentLoadedCallback(adaptedDoc, idSuffix);
    };

    rm._adaptIdAttributes = function(xml3dElement, idSuffix) {
        var that = this;
        var rootElementWithReplacedReferences = $(xml3dElement);
        var elementsWithSrcAttributes = rootElementWithReplacedReferences.find("[src]");
        var elementsWithShaderAttributes = rootElementWithReplacedReferences.find("[shader]");
        var elementsWithTransformAttributes = rootElementWithReplacedReferences.find("[transform]");

        elementsWithSrcAttributes.each( function(position, element) {
            that._applyReplacementToExternalSources(element, "src");
        });

        elementsWithShaderAttributes.each( function(position, element) {
            that._applyReplacementToExternalSources(element, "shader");
        });

        elementsWithTransformAttributes.each( function(position, element) {
            that._applyReplacementToExternalSources(element, "transform");
        });

        var elementsWithIdAttributes = rootElementWithReplacedReferences.find("[id]");
        elementsWithIdAttributes.each( function(position,element){
            that._applyReplacementToIdAttribute(element, rootElementWithReplacedReferences, idSuffix);
        } );
        return rootElementWithReplacedReferences;
    };

    rm._applyReplacementToExternalSources = function(element, attribute) {
        var srcValue = element.getAttribute(attribute);
        if(!srcValue || element.tagName === "img" || this._elementReferencesExternalFile(element, srcValue) )
        {
            return;
        }
        if(this._referencedElementIsAdaptedGlobal(srcValue))
        {
            var adaptedSourceValue = srcValue + "-" + this._assetIdSuffix;
            element.setAttribute(attribute, adaptedSourceValue);
        }
    };

    rm._referencedElementIsAdaptedGlobal = function(idReference) {
        var referencedID = idReference.slice(1, idReference.length);
        var adaptedReferencedId = referencedID + "-" + this._assetIdSuffix;
        var e = $("[id='"+adaptedReferencedId+"']");
        var defParents = e.parents("defs");
        return defParents.length > 0;
    };

    rm._elementReferencesExternalFile = function(element, srcValue) {
        return (srcValue.indexOf(".json") !== -1  || srcValue.indexOf(".xml") !== -1 );
    };

    rm._applyReplacementToIdAttribute = function(element, rootElement, idSuffix) {
        var e = $(element);
        var oldAttributeValue = e.attr("id");
        if(oldAttributeValue)
        {
            var adaptedAttributeValue = oldAttributeValue + "-" + idSuffix;
            e.attr("id", adaptedAttributeValue);
            if(element.tagName !== "group")
            {
                this._adaptReferencesFromOldToNew(oldAttributeValue, adaptedAttributeValue, rootElement);
            }
        }
    };

    rm._adaptReferencesFromOldToNew = function(oldId, newId,rootElement) {
        var srcReferencingObjects =	rootElement.find("[src='#"+oldId+"']");
        var transformReferencingObjects = rootElement.find("[transform='#"+oldId+"']");
        var shaderReferencingObjects = rootElement.find("[shader='#"+oldId+"']");
        var newReference = "#"+newId;

        srcReferencingObjects.each(function(position, element) {
            element.setAttribute("src", newReference);
        });
        transformReferencingObjects.each(function(position, element) {
            element.setAttribute("transform", newReference);
        });
        shaderReferencingObjects.each(function(position, element) {
            element.setAttribute("shader", newReference);
        });

    };

    FIVES.Resources.ResourceManager = new ResourceManager();
}());
