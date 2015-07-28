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
