var CARAMEL = CARAMEL || {};
CARAMEL.Utility = CARAMEL.Utility || {};


(function() {
    /** Loader for XML3D elements from a remote server using Ajax. 
     *  Call loadXML3D() with a url and a callback to be invoked, when 
     *  loading finished. 
     *  
     *  The Loader modifies all mesh's and img's src attributes. It 
     *  makes them absolute and point to the configured server. It 
     *  will also remove the leading "file:///" prefix if it's given.
     */
    CARAMEL.Utility.RemoteXML3DLoader = new XMOT.Singleton({
        
        /**
         * @param {string} url the url to load
         * @param {function(url: string, xml3d: <Object>)} loadedCB the callback to invoke when loading finished.
         */
        loadXML3D: function(fivesObject, loadedCB)
        {            
            var self = this;  
            
            $.ajax({
                type: "GET",
                url: fivesObject.mesh.uri,
                success: function(response) {
                    self._handleLoadedXML3D(fivesObject, response, loadedCB);
                },
                error: function(status) {console.error(status)}
            });
        }, 
        
        /** Convert all references to point to the correct server and 
         *  notify the load requester using loadedCB. 
         */
        _handleLoadedXML3D: function(fivesObject, loadedDocument, loadedCB)
        {
            var loadedXML3DEl = $(loadedDocument).children("xml3d")[0];

            // construct full path to the files by analysing urlOnServer
            var url = fivesObject.mesh.uri;
            var urlLastSlash = url.lastIndexOf("/"); 
            var urlPath = url.slice(0,  urlLastSlash + 1);
            this._adjustReferences(loadedXML3DEl, urlPath);
            // notify load requester
            loadedCB(loadedXML3DEl, fivesObject.guid);
        },
        
        /** Up to now convert all img's and mesh's elements' src attributes 
         *  to point to this.serverURL. It will remove the "file:///" prefix, 
         *  if given.  
         */
        _adjustReferences: function(node, baseURL)
        {
            // adjust all meshes and images 
            if(node.tagName === "mesh" || node.tagName === "img"  || node.tagName === "light" || node.tagName === "group" || node.tagName === "data")
            {
				this._adjustReferenceForAttribute(node, baseURL, "src");
				this._adjustReferenceForAttribute(node, baseURL, "shader");
				this._adjustReferenceForAttribute(node, baseURL, "transform");
            }

            // handle children
            _.each(node.childNodes, function(child) {
                this._adjustReferences(child, baseURL); 
            }, this);
        },

		_adjustReferenceForAttribute: function(node, baseURL, attribute)
		{
			var uriStr = $(node).attr(attribute);

			if(uriStr && uriStr.length > 0 && uriStr.indexOf("http://") < 0) // only replace relative paths
			{
				if(uriStr.indexOf("file:///") === 0)
				{
					uriStr = uriStr.slice("file:///".length);
				}

				if(uriStr.indexOf("#") !== 0)
				{
					$(node).attr(attribute, baseURL + uriStr);
				}
			}
		}
    });    
}());

