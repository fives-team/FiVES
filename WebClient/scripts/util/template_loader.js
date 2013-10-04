/**
 * Created with JetBrains WebStorm.
 * User: Torsten
 * Date: 2/14/13
 * Time: 8:44 AM
 * To change this template use File | Settings | File Templates.
 */
var CARAMEL = CARAMEL || {};

(function(){
	"use strict";
	CARAMEL.TemplateLoader = new XMOT.Singleton({

		loadLinkedTemplates: function(options) {
			if(options && options.success)
			{
				CARAMEL.TemplateLoader.successCallback = options.success;
			}

			var templateLinks = $("body").children("link[url]");
			CARAMEL.TemplateLoader.numTemplateLinks = templateLinks.length;
			CARAMEL.TemplateLoader.numTemplatesDone = 0;

			if(templateLinks.length > 0)
			{
				templateLinks.each(function(index, element) {
					CARAMEL.TemplateLoader.loadTemplateDocument($(element).attr("url"));
				});
			}
			else
			{
				CARAMEL.TemplateLoader.successCallback();
			}
		},

		loadTemplateDocument:  function(url) {
			$.ajax({
				url: url,
				method: "GET",
				header: {
					"Content-Type": "application/xml"
				},
				success: function(result) {
					CARAMEL.TemplateLoader.copyTemplatesToDOM(result);
				},
				error: function(status) {
					console.error("Error while fetching Template File " + url);
					console.error(status);
					CARAMEL.TemplateLoader.checkIfAllDocumentsLoaded();
				}
			});
		},

		copyTemplatesToDOM: function(result) {
			var scriptTags = $(result).children("script[type='text/html']");
			scriptTags.each(function(index, element) {
				$("body").append(element);
			});
			CARAMEL.TemplateLoader.checkIfAllDocumentsLoaded();
		},

		checkIfAllDocumentsLoaded: function() {
			this.numTemplatesDone ++;
			if(CARAMEL.TemplateLoader.numTemplatesDone === CARAMEL.TemplateLoader.numTemplateLinks)
			{
				CARAMEL.TemplateLoader.successCallback();
			}
		}

	});
}());
