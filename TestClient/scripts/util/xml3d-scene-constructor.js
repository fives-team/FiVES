var CARAMEL = CARAMEL || {};
CARAMEL.Utility = CARAMEL.Utility || {};

(function(){
	"use strict";

	/** The scene constructor manages the addition of new elements to a
	 * 	given xml3d element. It is a small wrapper for the XMOT.creation
	 * 	methods and provides methods such as addGroup(), addTransform() or addPointLight() so
	 * 	that the user doesn't have to take care about low-level details such as retrieving
	 * 	the defs element.
	 */
	CARAMEL.Utility.XML3DSceneConstructor = new XMOT.Class({

		initialize: function(xml3dElement) {
			this.xml3d = xml3dElement;
			this.$xml3d = $(this.xml3d);
			this.defs = XMOT.util.getOrCreateDefs(this.xml3d);
			this.$defs = $(this.defs);
		},

		/**
		 * 	@param {string} id
		 * 	@return {Element} the created <defs> element
		 */
		addDefs: function(id) {
			var defs = XMOT.creation.element("defs", {id: id});
			this.$xml3d.append(defs);
			return defs;
		},

		/**
		 * 	@param {Object} attributes that are passed to XMOT.creation.element
		 * 	@return {Element} the created <group> element
		 */
		addGroup: function(attributes){
			var group = XMOT.creation.element("group", attributes);
			this.$xml3d.append(group);
			return group;
		},

		/**
		 * 	@param {Object} attributes that are passed to XMOT.creation.element
		 * 	@return {Element} the created <transform> element
		 */
		addTransform: function(attributes) {
			var transform = XMOT.creation.element("transform", attributes);
			this.$defs.append(transform);
			return transform;
		},

		/**
		 * 	@param {XML3DVec3} position
		 * 	@param {XML3DVec3=} intensity, default (0.8, 0.8, 0.8)
		 * 	@param {XML3DVec3=} attenuation, default (1,0.01,0)
		 *	@return {Element} the created <light> element
		 */
		addPointLight: function(position, intensity, attenuation) {

			if(!intensity)
				intensity = new XML3DVec3(0.8, 0.8, 0.8);
			if(!attenuation)
				attenuation = new XML3DVec3(1, 0.01, 0);

			var ids = this._generateLightIDs();

			var shader = XMOT.creation.lightshaderPoint({
				id: ids.lightshader,
				inten: intensity.str(),
				atten: attenuation.str()
			});
			this.$defs.append(shader);

			this.addTransform({id: ids.transform, translation: position.str()});

			return this._addLight(ids);
		},

		/**
		 * 	@param {XML3DVec3} direction
		 * 	@param {XML3DVec3=} intensity, default (0.8, 0.8, 0.8)
		 *	@return {Element} the created <light> element
		 */
		addDirectionalLight: function(direction, intensity) {

			if(!intensity)
				intensity = new XML3DVec3(0.8, 0.8, 0.8);

			var ids = this._generateLightIDs();

			var shader = XMOT.creation.lightshaderDirectional({
				id: ids.lightshader,
				inten: intensity.str()
			});
			this.$defs.append(shader);

			var defaultDir = new XML3DVec3(0,0,-1); // from spec
			var orientation = new XML3DRotation();
			orientation.setRotation(defaultDir, direction);

			this.addTransform({id: ids.transform, rotation: orientation.str()});

			return this._addLight(ids);
		},

		/** Add a spot light to the scene with default intensity (0.8, 0.8, 0.8).
		 *
		 * 	@param {XML3DVec3} position
		 * 	@param {XML3DVec3} direction
		 * 	@param {XML3DVec3=} intensity, default (0.8, 0.8, 0.8)
		 * 	@param {XML3DVec3=} attenuation, default (1,0.01,0)
		 * 	@param {number=} fallOffAngle, default 0.8
		 * 	@param {number=} softness, default 1
		 *	@return {Element} the created <light> element
		 */
		addSpotLight: function(position, direction, intensity, attenuation, fallOffAngle, softness) {

			if(!intensity)
				intensity = new XML3DVec3(0.8, 0.8, 0.8);
			if(!attenuation)
				attenuation = new XML3DVec3(1, 0.01, 0);
			if(!fallOffAngle)
				fallOffAngle = 0.8;
			if(!softness)
				softness = 1;

			var ids = this._generateLightIDs();

			var shader = XMOT.creation.lightshaderSpot({
				id: ids.lightshader,
				inten: intensity.str(),
				atten: attenuation.str(),
				falloff: fallOffAngle,
				soft: softness
			});
			this.$defs.append(shader);

			var defaultDir = new XML3DVec3(0,0,-1); // from spec
			var orientation = new XML3DRotation();
			orientation.setRotation(defaultDir, direction);

			this.addTransform({
				id: ids.transform,
				translation: position.str(),
				rotation: orientation.str()
			});

			return this._addLight(ids);
		},

		/**
		 * 	@param {string} id
		 * 	@param {string} emissiveColor
		 * 	@param {number=} transparency, default: will not be set
		 * 	@return {Element} the created <shader> element
		 */
		addEmissiveColoredPhongShader: function(id, emissiveColor, transparency) {

			var emColDataSrc = XMOT.creation.dataSrc("float3", {
				name: "emissiveColor",
				val: emissiveColor
			});

			var shader = XMOT.creation.element("shader", {
				id: id,
				script: "urn:xml3d:shader:phong",
				children: [emColDataSrc]
			});

			if(transparency !== undefined) {

				var transpDataSrc = XMOT.creation.dataSrc("float", {
					name: "transparency",
					val: transparency
				});
				shader.appendChild(transpDataSrc);
			}

			this.$defs.append(shader);
			return shader;
		},

		_addLight: function(lightIDs) {

			var light = XMOT.creation.element("light", {
				shader: "#" + lightIDs.lightshader
			});

			this.$xml3d.append(XMOT.creation.element("group", {
				transform: "#" + lightIDs.transform,
				children: [light]
			}));

			return light;
		},

		_generateLightIDs: function() {
			var id = CARAMEL.Utility.IDGenerator.newID();

			return {
				lightshader: "ls_" + id,
				transform: "t_light_" + id
			};
		}
	});
}());