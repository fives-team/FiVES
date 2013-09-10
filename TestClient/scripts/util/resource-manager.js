var CARAMEL = CARAMEL || {};
CARAMEL.Utility = CARAMEL.Utility || {};

(function(){
	"use strict";
	/** TODO: desc
	*
	*	Events:
	*	load:add (id, url)
	*	load:addsubdoc (parentId, url, parenturl)
	*
	*	load:donedoc (id, url, xml3d) triggered when loading of the document itself is done
	*	load:donesubdoc (id, url, parenturl) triggered when loading of a sub document is done
	*	load:done (id, url, xml3d) triggered when loading of the document and all sub documents is done
	*
	*	load:alldone everything is loaded
	*/
	CARAMEL.Utility.ResourceManager = new XMOT.Singleton({

		initialize: function() {
			/** should be set from outside */
			this.xml3dElement = null;

			/** Storage for loaded URLs including the sub URLs to the JSON
			 *  meshes.
			 *  
			 *  map: id -> {id: <string>, url: <string>, loadedXml3d: <Object>, subDocs: Array.<string>} 
			 */
			this._loadingDocuments = {};

			this._loadedURLs = [];

			XML3D.xmlHttpCallback = _.bind(this._onXML3DXHR, this);
		},

		loadedURL: function(url) {
			return _.contains(this._loadedURLs, url);
		},

		loadXML3DDocument: function(id, url) {
			var doc = this._addDocument(id, url, true);
			this._startLoading(doc);
		},

		loadXML3DScene: function(id, url, wait2Frames) {
			var doc = this._addDocument(id, url, false, wait2Frames);
			this._startLoading(doc);
		},

		loadAssetInstanceDocument: function(id, assetId,  url) {
			var doc = this._addDocument(id, url, false, true); /* doc id, asset url, loadDocOnly, wait2frames */
			doc._assetIdSuffix = assetId;
			doc._instanceIdSuffix = id;
			doc.type = "assetinstance";
			this._startLoading(doc);
		},

		exportXML3DScene: function(callback) {
			var serializedXml3d = (new XMLSerializer()).serializeToString(this.xml3dElement);
			var scenarioName = CARAMEL.selectedScenario.get("name");
			var fileName = scenarioName.replace(" ","_");
			$.ajax({
					type: "POST",
					url: "script/php/export_scene.php",
					data: {
						filename: fileName,
						scene: serializedXml3d
					},
					success: function(result) {
						callback(result);
					},
					error: function(result) {
						console.warn(result);
					}
				}
			);
		},


		_startLoading: function(doc) {
			var fn = _.bind(function(url, xml3dElement) {
				this._onLoadedDocument(doc, xml3dElement);
			}, this);

			CARAMEL.Utility.RemoteXML3DLoader.loadXML3D(doc.url, fn);
		},

		_addDocument: function(id, url, loadDocOnly, wait2Frames) {
			var doc = {
				id: id,
				url: url,
				loadedXml3d: null,
				loadDocOnly: loadDocOnly,
				wait2Frames: wait2Frames,
				subDocs: []
			};

			this._loadingDocuments[id] = doc;

			this._triggerAdd(doc);

			return doc;
		},

		_addSubDocuments: function(doc, subUrls) {
			for(var i = 0; i < subUrls.length; i++) {
				var subDocToAdd = subUrls[i];
				if(doc.subDocs.indexOf(subDocToAdd) < 0)
				{
					doc.subDocs.push(subDocToAdd);
					this._triggerAddSubDoc(doc, subDocToAdd);
				}
			}
		},

		_doneDocument: function(doc, xml3dElement) {
			doc.loadedXml3d = xml3dElement;
			if(doc.type === "assetinstance") {
				this._finalizeLoadedInstanceDocument(doc);
			}
			this._triggerDoneDoc(doc);
		},

		_doneSubDocument: function(subUrl) {
			// remove sub document from every document
			for(var id in this._loadingDocuments)
			{
				var doc = this._loadingDocuments[id];
				var docsThatMatch = _.filter(doc.subDocs, this._isTailOf(subUrl));

				if(docsThatMatch.length < 1)
				{
					continue;
				}
				this._removeSubDocument(doc, docsThatMatch[0]);
				this._triggerDoneSubDoc(doc, docsThatMatch[0]);

				if(doc.subDocs.length < 1)
				{
					this._finishDocument(doc);
				}
			}
		},

		_isTailOf: function (someString) {
			// return an "iterator" test function for some()
			return function (candidate) {
				return (someString.indexOf(candidate, someString.length - candidate.length) !== -1);
			};
		},

		_finishDocument: function(doc) {
			var xml3d = this.xml3dElement;
			if(!xml3d) {
				throw new Error("ResourceManager: no xml3d element specified.");
			}
			var fn = _.bind(function() {
				this._finalizeLoadedDocument(doc);
			}, this);

			$(xml3d).one("framedrawn", fn);
		},

		_checkIfAllDocumentsDone: function() {
			var allDocs = Object.keys(this._loadingDocuments);
			if(allDocs.length < 1) {
				this._triggerAllDone();
			}
		},

		_finalizeLoadedDocument: function(doc) {
			delete this._loadingDocuments[doc.id];

			this._loadedURLs.push(doc.url);
			this._triggerDone(doc);
			this._checkIfAllDocumentsDone();
		},

		_finalizeLoadedInstanceDocument: function(doc) {
			this._adaptIdAttributes(doc);
			this._finalizeLoadedDocument(doc);
		},

		// --- Callbacks --- 

		/** Called by CARAMEL.Utility.RemoteXML3DLoader when loading finished.
		 */
		_onLoadedDocument: function(doc, xml3dElement) {
			var meshRefs = this._getJSONMeshReferences(xml3dElement);

			var urlLoadedBefore = this.loadedURL(doc.url);
			var doFastFinish = doc.loadDocOnly || meshRefs.length < 1;
			var doSubDocs = !doFastFinish && !urlLoadedBefore;

			if(doSubDocs) {
				this._addSubDocuments(doc, meshRefs);
			}
			this._doneDocument(doc, xml3dElement);

			if(doFastFinish) {
				this._finalizeLoadedDocument(doc);	// early finish w/o waiting for framedrawn
			}
			else if(urlLoadedBefore) {
				this._finishDocument(doc);
			}
		},

		_adaptIdAttributes: function(doc) {
			this._assetIdSuffix = doc._assetIdSuffix;
			this._instanceIdSuffix = doc._instanceIdSuffix;
			var that = this;
			var rootElementWithReplacedReferences = $(doc.loadedXml3d);
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
				that._applyReplacementToIdAttribute(element, rootElementWithReplacedReferences);
			} );
			doc.loadedXml3d = rootElementWithReplacedReferences.get(0); // get(0) to transform from jquery back to DOM representation
		},

		_applyReplacementToExternalSources: function(element, attribute) {
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
		},

		_referencedElementIsAdaptedGlobal: function(idReference) {
			var referencedID = idReference.slice(1, idReference.length);
			var adaptedReferencedId = referencedID + "-" + this._assetIdSuffix;
			var e = $("[id='"+adaptedReferencedId+"']");
			var defParents = e.parents("defs");
			return defParents.length > 0;
		},

		_elementReferencesExternalFile: function(element, srcValue) {
			return (srcValue.indexOf(".json") !== -1  || srcValue.indexOf(".xml") !== -1 );
		},

		_applyReplacementToIdAttribute: function(element, rootElement) {
			var e = $(element);
			var oldAttributeValue = e.attr("id");
			if(oldAttributeValue)
			{
				var adaptedAttributeValue = oldAttributeValue + "-" + this._instanceIdSuffix;
				e.attr("id", adaptedAttributeValue);
				if(element.tagName !== "group")
				{
					this._adaptReferencesFromOldToNew(oldAttributeValue, adaptedAttributeValue, rootElement);
				}
			}
		},
		_adaptReferencesFromOldToNew: function(oldId, newId,rootElement) {
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

		},

		_onXML3DXHR: function(xmlHttpRequest) {
			this._doneSubDocument(xmlHttpRequest._url);
		},

		// --- Event triggering methods ---

		/** load:add (id, url) */
		_triggerAdd: function(doc) {
			CARAMEL.App.vent.trigger("load:add", doc.id, doc.url);
		},

		/** load:addsubdoc (id, url, parenturl) */
		_triggerAddSubDoc: function(doc, url) {
			CARAMEL.App.vent.trigger("load:addsubdoc", doc.id, url, doc.url);
		},

		/** load:donedoc (id, url, xml3d) */
		_triggerDoneDoc: function(doc) {
			var clonedNode = doc.loadedXml3d.cloneNode(true);
			CARAMEL.App.vent.trigger("load:donedoc", doc.id, doc.url, clonedNode);
		},

		/** load:donesubdoc (id, url, parenturl) */
		_triggerDoneSubDoc: function(doc, subUrl) {
			CARAMEL.App.vent.trigger("load:donesubdoc", doc.id, subUrl, doc.url);
		},

		/** load:done (id, url, xml3d) */
		_triggerDone: function(doc) {
			var clonedNode = doc.loadedXml3d.cloneNode(true);
			CARAMEL.App.vent.trigger("load:done", doc.id, doc.url, clonedNode);
		},

		/** load:alldone */
		_triggerAllDone: function() {
			CARAMEL.App.vent.trigger("load:alldone");
		},

		// --- Utilities ---
		_removeSubDocument: function(doc, url) {

			var subDocIdx = _.indexOf(doc.subDocs, url);
			if(subDocIdx > -1) {
				doc.subDocs.splice(subDocIdx, 1);
			}
		},

		_getJSONMeshReferences: function(element) {
			return $(element).find("mesh[src$='.json']").map(
				function(){
					return this.getAttribute("src");
				}
			).get();
		}
	});
}());
