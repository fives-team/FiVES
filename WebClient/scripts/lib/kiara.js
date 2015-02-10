define(function () {
    // Browser compatibility layer
    if (typeof Object.create !== 'function') {
        Object.create = function (o) {
            if (arguments.length > 1) {
                throw new Error('Object.create implementation only accepts the first parameter.');
            }
            function F() {}
            F.prototype = o;
            return new F();
        };
    }

    var isNode = (typeof process === 'object' && typeof require === 'function');
    var isWeb = typeof window === 'object';
    var isWorker = typeof importScripts === 'function';

    // Node.js compatibility layer
    if (isNode) {
        var util = require('util');
    } else {
        var util = {};
        util.inherits = function(ctor, superCtor) {
            ctor.super_ = superCtor;
            ctor.prototype = Object.create(superCtor.prototype);
            ctor.prototype.constructor = ctor;
            ctor.superclass = superCtor.prototype;
        };
    }

    var KIARA = {};
    KIARA.inherits = util.inherits;

    // Utilities

    function isNumber(value) {
        return Object.prototype.toString.call(value) === '[object Number]';
    }
    function isString(value) {
        return Object.prototype.toString.call(value) === '[object String]';
    }
    function isBoolean(value) {
        return value === true || value === false || Object.prototype.toString.call(value) === '[object Boolean]';
    }
    function isFunction(value) {
        return Object.prototype.toString.call(value) === '[object Function]';
    }
    if (typeof (/./) !== 'function') {
        function isFunction(value) {
            return typeof value === 'function';
        }
    }
    function isArray(value) {
        return Object.prototype.toString.call(value) === '[object Array]';
    }
    function isObject(value) {
        return Object.prototype.toString.call(value) === '[object Object]';
    }
    KIARA.isNumber = isNumber;
    KIARA.isString = isString;
    KIARA.isBoolean = isBoolean;
    KIARA.isFunction = isFunction;
    KIARA.isArray = isArray;
    KIARA.isObject = isObject;

    // Data loading

    var binaryContentTypes = ["application/octet-stream"];
    var binaryExtensions = [".bin", ".bson"];

    function endsWith(str, suffix) {
        return str.indexOf(suffix, str.length - suffix.length) !== -1;
    }

    function isBinaryExtension(url) {
        for (var i in binaryExtensions) {
            if (endsWith(url, binaryExtensions[i]))
                return true;
        }
        return false;
    }

    function isBinaryContentType(contentType) {
        for (var i in binaryContentTypes) {
            if (contentType == binaryContentTypes[i]) {
                return true;
            }
        }
        return false;
    }

    /**
     * Load data via XMLHttpRequest
     * @private
     * @param {string} url URL of the document
     * @param {function} loadListener function called with loaded data
     * @param {function} errorListener function called with XMLHttpRequest when load failed,
     *                                 _url attribute of the XMLHttpRequest contains URL of the request
     */
    function loadData(url, loadListener, errorListener) {
        var xmlHttp = null;
        try {
            xmlHttp = new XMLHttpRequest();
        } catch (e) {
            xmlHttp = null;
        }
        if (xmlHttp) {
            xmlHttp._url = url;
            xmlHttp._contentChecked = false;
            xmlHttp.open('GET', url, true);
            if (isBinaryExtension(url))
                xmlHttp.responseType = "arraybuffer";

            xmlHttp.onreadystatechange = function() {
                if (xmlHttp._aborted) // This check is possibly not needed
                    return;
                // check compatibility between content and request mode
                if (!xmlHttp._contentChecked &&
                    // 2 - HEADERS_RECEIVED, 3 - LOADING, 4 - DONE
                    ((xmlHttp.readyState == 2 || xmlHttp.readyState == 3 ||xmlHttp.readyState == 4) &&
                        xmlHttp.status == 200)) {
                    xmlHttp._contentChecked = true; // we check only once
                    // check if we need to change request mode
                    var contentType = xmlHttp.getResponseHeader("content-type");
                    if (contentType) {
                        var binaryContent = isBinaryContentType(contentType);
                        var binaryRequest = (xmlHttp.responseType == "arraybuffer");
                        // When content is not the same as request, we need to repeat request
                        if (binaryContent != binaryRequest) {
                            xmlHttp._aborted = true;
                            var cb = xmlHttp.onreadystatechange;
                            xmlHttp.onreadystatechange = null;
                            var url = xmlHttp._url;
                            xmlHttp.abort();

                            // Note: We do not recycle XMLHttpRequest !
                            //       This does work only when responseType is changed to "arraybuffer",
                            //       however the size of the xmlHttp.response buffer is then wrong !
                            //       It does not work at all (at least in Chrome) when we use overrideMimeType
                            //       with "text/plain; charset=x-user-defined" argument.
                            //       The latter mode require creation of the fresh XMLHttpRequest.

                            xmlHttp = new XMLHttpRequest();
                            xmlHttp._url = url;
                            xmlHttp._contentChecked = true;
                            xmlHttp.open('GET', url, true);
                            if (binaryContent)
                                xmlHttp.responseType = "arraybuffer";
                            xmlHttp.onreadystatechange = cb;
                            xmlHttp.send(null);
                            return;
                        }
                    }
                }
                // Request mode and content type are compatible here (both binary or both text)
                if (xmlHttp.readyState == 4) {
                    if(xmlHttp.status == 200) {
                        var mimetype = xmlHttp.getResponseHeader("content-type");
                        var response = null;

                        if (xmlHttp.responseType == "arraybuffer") {
                            response = xmlHttp.response;
                        } else if (mimetype == "application/json") {
                            response = JSON.parse(xmlHttp.responseText);
                        } else if (mimetype == "application/xml" || mimetype == "text/xml") {
                            response = xmlHttp.responseXML;
                        } else {
                            response = xmlHttp.responseText;
                        }
                        if (loadListener)
                            loadListener(response);
                    } else {
                        //console.error("Could not load external document '" + xmlHttp._url +
                        //    "': " + xmlHttp.status + " - " + xmlHttp.statusText);
                        if (errorListener)
                            errorListener(xmlHttp);
                    }
                }
            };
            xmlHttp.send(null);
        }
    };

    KIARA.SUCCESS = 0;
    KIARA.FALSE = false;
    KIARA.TRUE = true;

    /** Return codes signaling errors */

    KIARA.NO_ERROR          = KIARA.SUCCESS;
    KIARA.GENERIC_ERROR     = 0x0001;
    KIARA.INPUT_ERROR       = 0x0100;
    KIARA.OUTPUT_ERROR      = 0x0200;
    KIARA.CONNECTION_ERROR  = 0x0300;
    KIARA.IDL_LOAD_ERROR    = 0x0301;
    KIARA.API_ERROR         = 0x0500;
    KIARA.INIT_ERROR        = 0x0501;
    KIARA.FINI_ERROR        = 0x0502;
    KIARA.INVALID_VALUE     = 0x0503;
    KIARA.INVALID_TYPE      = 0x0504;
    KIARA.INVALID_OPERATION = 0x0505;
    KIARA.INVALID_ARGUMENT  = 0x0506;
    KIARA.UNSUPPORTED_FEATURE = 0x0507;

    /** Return codes from a function call */

    KIARA.EXCEPTION         = 0x1000;

    var errorMsg = {};
    errorMsg[KIARA.NO_ERROR] = 'No error';
    errorMsg[KIARA.GENERIC_ERROR] = 'Generic error';
    errorMsg[KIARA.INPUT_ERROR] = 'Input error';
    errorMsg[KIARA.OUTPUT_ERROR] = 'Output error';
    errorMsg[KIARA.CONNECTION_ERROR] = 'Connection error';
    errorMsg[KIARA.API_ERROR] = 'API error';
    errorMsg[KIARA.INIT_ERROR] = 'Init error';
    errorMsg[KIARA.FINI_ERROR] = 'Finalization error';
    errorMsg[KIARA.INVALID_VALUE] = 'Invalid value';
    errorMsg[KIARA.INVALID_TYPE] = 'Invalid type';
    errorMsg[KIARA.INVALID_OPERATION] = 'Invalid operation';
    errorMsg[KIARA.INVALID_ARGUMENT] = 'Invalid argument';

    // -- KIARA.Error / KIARAError --

    function KIARAError(errorCode, message) {
        if (Error.captureStackTrace) // V8
            Error.captureStackTrace(this, this.constructor); //super helper method to include stack trace in error object
        else
            this.stack = (new Error).stack;

        this.name = this.constructor.name;
        this.errorCode = errorCode || KIARA.GENERIC_ERROR;
        this.message = message || errorMsg[this.errorCode];
    }
    KIARAError.prototype = new Error();
    KIARAError.prototype.constructor = KIARAError;
    KIARA.Error = KIARAError;

    // Hashing

    // From boost::hash_combine
    function combineHashValue(seed, hashValue) {
        seed ^= hashValue + 0x9e3779b9 + ((seed << 6) >>> 0) + (seed >>> 2);
        return seed >>> 0;
    }

    function hashValue(v) {
        if (v === null || v === undefined)
            return 0;
        if (v.hash)
            return v.hash();
        else if (isArray(v)) {
            seed = 0;
            for (var i = 0; i < v.length; ++i) {
                seed = hashCombine(seed, v[i]);
            }
            return seed;
        } else if (v.charCodeAt) { // String
            seed = 0;
            for (var i = 0; i < v.length; ++i) {
                seed = combineHashValue(seed, v.charCodeAt(i));
            }
            return seed;
        } else if (v === true) {
            return 1;
        } else if (v === false) {
            return 0;
        } else if (isNumber(v)) {
            if (v === 0) {
                return 0;
            } else if (v === Number.POSITIVE_INFINITY) {
                return -1>>>0;
            } else if (v === Number.NEGATIVE_INFINITY) {
                return -2>>>0;
            } else if (v != v) {
                // not a number
                return -3>>>0;
            } else if (v > 0 && (v>>>0) === v) { // check for 32-bit positive integer
                return v;
            } else if (v < 0 && (v|0) === v) { // check for 32-bit negative integer
                return v>>>0; // convert to unsigned int
            }
            // TODO compute hash from float
            // look in boost/functional/hash/detail/hash_float_generic.hpp
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Cannot compute hash from: "+v);
        }

        throw new KIARAError(KIARA.INVALID_ARGUMENT, "Cannot compute hash from: "+toString.call(v));
    }

    function hashCombine(seed, v) {
        return combineHashValue(seed, hashValue(v));
    }


    // Enums

    var NodeKind = {};

    NodeKind.FIRST_NODE_KIND = 0;
    NodeKind.LAST_NODE_KIND = NodeKind.FIRST_NODE_KIND;
    function nextNodeKind(i) {
        if (i !== undefined) {
            NodeKind.LAST_NODE_KIND = i;
            return i;
        }
        return ++NodeKind.LAST_NODE_KIND;
    }

    NodeKind.FIRST_PRIMTYPE_NODE = nextNodeKind(NodeKind.FIRST_NODE_KIND);
    NodeKind.NODE_PRIMTYPE_i8 = nextNodeKind(NodeKind.FIRST_PRIMTYPE_NODE);
    NodeKind.NODE_PRIMTYPE_u8 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_i16 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_u16 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_i32 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_u32 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_i64 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_u64 = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_float = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_double = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_boolean = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_string = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_int8_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_uint8_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_int16_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_uint16_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_int32_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_uint32_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_int64_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_uint64_t = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_float = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_double = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_c_longdouble = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_js_number = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_js_string = nextNodeKind();
    NodeKind.NODE_PRIMTYPE_js_boolean = nextNodeKind();
    //NodeKind.LAST_PRIMTYPE_NODE = nextNodeKind(NodeKind.NODE_PRIMTYPE_c_longdouble);
    NodeKind.LAST_PRIMTYPE_NODE = nextNodeKind(NodeKind.NODE_PRIMTYPE_js_boolean);
    NodeKind.FIRST_C_PRIMTYPE_NODE = nextNodeKind(NodeKind.NODE_PRIMTYPE_c_int8_t);
    NodeKind.NUM_PRIMTYPES = nextNodeKind(NodeKind.LAST_PRIMTYPE_NODE - NodeKind.FIRST_PRIMTYPE_NODE);
    NodeKind.NODE_VOIDTYPE = nextNodeKind(NodeKind.LAST_PRIMTYPE_NODE + 1);
    NodeKind.NODE_PRIMVALUETYPE = nextNodeKind();
    NodeKind.NODE_UNRESOLVEDSYMBOLTYPE = nextNodeKind();
    NodeKind.NODE_SYMBOLTYPE = nextNodeKind();
    NodeKind.NODE_ANYTYPE = nextNodeKind();
    NodeKind.NODE_TYPETYPE = nextNodeKind();
    NodeKind.NODE_PTRTYPE = nextNodeKind();
    NodeKind.NODE_REFTYPE = nextNodeKind();
    NodeKind.NODE_ARRAYTYPE = nextNodeKind();
    NodeKind.NODE_FIXEDARRAYTYPE = nextNodeKind();
    NodeKind.NODE_STRUCTTYPE = nextNodeKind();
    NodeKind.NODE_FUNCTYPE = nextNodeKind();
    NodeKind.NODE_SERVICETYPE = nextNodeKind();
    NodeKind.NODE_VARIANTTYPE = nextNodeKind();

    var PrimTypeKind = {
        PRIMTYPE_i8     : NodeKind.NODE_PRIMTYPE_i8,
        PRIMTYPE_u8     : NodeKind.NODE_PRIMTYPE_u8,
        PRIMTYPE_i16    : NodeKind.NODE_PRIMTYPE_i16,
        PRIMTYPE_u16    : NodeKind.NODE_PRIMTYPE_u16,
        PRIMTYPE_i32    : NodeKind.NODE_PRIMTYPE_i32,
        PRIMTYPE_u32    : NodeKind.NODE_PRIMTYPE_u32,
        PRIMTYPE_i64    : NodeKind.NODE_PRIMTYPE_i64,
        PRIMTYPE_u64    : NodeKind.NODE_PRIMTYPE_u64,
        PRIMTYPE_float      : NodeKind.NODE_PRIMTYPE_float,
        PRIMTYPE_double     : NodeKind.NODE_PRIMTYPE_double,
        PRIMTYPE_boolean    : NodeKind.NODE_PRIMTYPE_boolean,
        PRIMTYPE_string     : NodeKind.NODE_PRIMTYPE_string,
        PRIMTYPE_c_int8_t   : NodeKind.NODE_PRIMTYPE_c_int8_t,
        PRIMTYPE_c_uint8_t  : NodeKind.NODE_PRIMTYPE_c_uint8_t,
        PRIMTYPE_c_int16_t  : NodeKind.NODE_PRIMTYPE_c_int16_t,
        PRIMTYPE_c_uint16_t : NodeKind.NODE_PRIMTYPE_c_uint16_t,
        PRIMTYPE_c_int32_t  : NodeKind.NODE_PRIMTYPE_c_int32_t,
        PRIMTYPE_c_uint32_t : NodeKind.NODE_PRIMTYPE_c_uint32_t,
        PRIMTYPE_c_int64_t  : NodeKind.NODE_PRIMTYPE_c_int64_t,
        PRIMTYPE_c_uint64_t : NodeKind.NODE_PRIMTYPE_c_uint64_t,
        PRIMTYPE_c_float    : NodeKind.NODE_PRIMTYPE_c_float,
        PRIMTYPE_c_double   : NodeKind.NODE_PRIMTYPE_c_double,
        PRIMTYPE_c_longdouble : NodeKind.NODE_PRIMTYPE_c_longdouble,
        PRIMTYPE_js_number : NodeKind.NODE_PRIMTYPE_js_number,
        PRIMTYPE_js_string : NodeKind.NODE_PRIMTYPE_js_string,
        PRIMTYPE_js_boolean : NodeKind.NODE_PRIMTYPE_js_boolean,
        FIRST_C_PRIMTYPE    : NodeKind.FIRST_C_PRIMTYPE_NODE
    };

    // -- KIARA.Object --

    var nextGlobalID = 0;
    function generateID() {
        return nextGlobalID++;
    }

    function KIARAObject(world) {
        checkWorld(world);
        this._id = generateID();
        this._world = world;
    }
    KIARAObject.prototype.getClassName = function() {
        return this._className;
    }
    KIARAObject.prototype.getWorld = function() {
        return this._world;
    }
    KIARAObject.prototype.hash = function() {
        return this._id;
    }
    KIARAObject.prototype.toString = function() {
        return "["+this._className+" "+this._id+"]";
    }
    KIARAObject.prototype.dump = function() {
        console.log(this.toString());
    }
    KIARAObject.prototype.equals = function(other) {
        return this === other;
    }
    KIARAObject.prototype._className = "KIARA.Object";

    KIARA.Object = KIARAObject;

    // -- KIARA.Namespace --

    function Namespace(world, name) {
        KIARAObject.call(this, world);
        this.name = name;
        this.parent = null;
        this.typeMap = {};
        this.subnamespaces = [];
    }
    util.inherits(Namespace, KIARAObject);
    Namespace.prototype._className = "KIARA.Namespace";

    Namespace.prototype.getName = function() {
        return this.name;
    }

    Namespace.prototype.getFullName = function() {
        var fullName = "", names = [];
        var ns = this.parent;
        while (ns)
        {
            names.push(ns.getName());
            ns = ns.getParent();
        }
        for (var i = names.length; i--; ) {
            fullName += names[i];
            fullName += '.';
        }
        fullName += this.getName();
        return fullName;
    }

    Namespace.prototype.getParent = function() { return this._parent; }
    Namespace.prototype.setParent = function(parent) {
        this.parent = parent;
    }
    Namespace.prototype.bindType = function(name, type, takeOwnership) {
        if (!type)
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "type can't be null or undefined");
        if (type.getWorld() != this.getWorld())
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "type's world must be the same as namespace world");
        if (this.typeMap.hasOwnProperty(name))
            throw new KIARAError(KIARA.INVALID_ARGUMENT,"Type '"+name+"' already defined.");

        this.typeMap[name] = type;
        if (takeOwnership || takeOwnership === undefined) {
            type.setNamespace(this);
        }
    }
    Namespace.prototype.lookupType = function(name) {
       return this.typeMap[name];
    }
    Namespace.prototype.getTypeName = function(type) {
        for (var typeName in this.typeMap) {
            if (this.typeMap[typeName] === type)
                return typeName;
        }
        return "";
    }
    Namespace.prototype.toString = function() {
        return "Namespace("+this.name+")";
    }
    Namespace.prototype.getTypeMap = function() {
        return this.typeMap;
    }
    KIARA.Namespace = Namespace;


    // -- KIARA.Type --

    function Type(world, name, kind, numOrElems) {
        KIARAObject.call(this, world);
        this.name = name;
        this.kind = kind;
        this.namespace = null;
        if (numOrElems instanceof Array)
            this.elements = numOrElems.slice(0);
        else if (numOrElems)
            this.elements = new Array(numOrElems);
        else
            this.elements = [];
    }
    util.inherits(Type, KIARAObject);
    Type.prototype._className = "KIARA.Type";

    Type.prototype.getNamespace = function() { return this.namespace; }
    Type.prototype.setNamespace = function(newNamespace) { this.namespace = newNamespace; }

    Type.prototype.getTypeName = function() { return this.name; }

    Type.prototype.getFullTypeName = function() {
        if (this.namespace)
            return this.namespace.getFullName() + "." + this.getTypeName();
        else
            return this.getTypeName();
    }

    Type.prototype.getKind = function() { return this.kind; }

    Type.prototype.getElements = function() { return this.elements; }

    Type.prototype.getElementAt = function(index) { return this.elements[index]; }

    Type.prototype.getNumElements = function() { return this.elements.length; }

    Type.prototype.equals = function(other) {
        if (!other || !Type.prototype.isPrototypeOf(other))
            return false;
        if (this.getKind() !== other.getKind())
            return false;
        if (this.elements.length != other.elements.length)
            return false;
        for (var i = 0; i < this.elements.length; ++i)
        {
            if (this.elements[i] === other.elements[i])
                continue;

            if (this.elements[i] && this.elements[i].equals(other.elements[i]))
                continue;

            return false;
        }
        return true;
    }

    Type.prototype.hash = function() {
        var seed = 0;
        seed = hashCombine(seed, this.getKind());
        seed = hashCombine(seed, this.elements);
        return seed;
    }

    Type.prototype.toString = function() {
        return this.getFullTypeName();
    }

    Type.prototype.resizeElements = function(newSize) {
        this.elements.length = newSize;
    }

    KIARA.Type = Type;

    // -- PrimType --

    var NameOfPrimTypeKind = {};
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i8] = "i8";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u8] = "u8";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i16] = "i16";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u16] = "u16";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i32] = "i32";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u32] = "u32";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i64] = "i64";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u64] = "u64";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_float] = "float";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_double] = "double";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_boolean] = "boolean";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_string] = "string";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int8_t] = "c_int8_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint8_t] = "c_uint8_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int16_t] = "c_int16_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint16_t] = "c_uint16_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int32_t] = "c_int32_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint32_t] = "c_uint32_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int64_t] = "c_int64_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint64_t] = "c_uint64_t";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_float] = "c_float";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_double] = "c_double";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_longdouble] = "c_longdouble";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_js_number] = "js_number";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_js_string] = "js_string";
    NameOfPrimTypeKind[PrimTypeKind.PRIMTYPE_js_boolean] = "js_boolean";

    var ByteSizeOfPrimTypeKind = {};
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i8] = 1;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u8] = 1;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i16] = 2;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u16] = 2;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i32] = 4;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u32] = 4;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_i64] = 8;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_u64] = 8;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_float] = 4;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_double] = 8;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_boolean] = 1; //???
    //ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_string] = ???
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int8_t] = 1;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint8_t] = 1;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int16_t] = 2;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint16_t] = 2;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int32_t] = 4;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint32_t] = 4;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_int64_t] = 8;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_uint64_t] = 8;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_float] = 4;
    ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_double] = 8;
    //ByteSizeOfPrimTypeKind[PrimTypeKind.PRIMTYPE_c_longdouble] = ???

    function PrimType(world, kind) {
        Type.call(this, world, NameOfPrimTypeKind[kind], kind, 0);
    }
    util.inherits(PrimType, Type)
    PrimType.prototype._className = "KIARA.PrimType";

    PrimType.getNameOfPrimTypeKind = function(kind) {
        return NameOfPrimTypeKind[kind];
    }

    PrimType.getByteSizeOfPrimTypeKind = function(kind) {
        return ByteSizeOfPrimTypeKind[kind];
    }

    PrimType.isIntegerPrimTypeKind = function(kind) {
        return (kind === PrimTypeKind.PRIMTYPE_i8 ||
            kind === PrimTypeKind.PRIMTYPE_u8 ||
            kind === PrimTypeKind.PRIMTYPE_i16 ||
            kind === PrimTypeKind.PRIMTYPE_u16 ||
            kind === PrimTypeKind.PRIMTYPE_i32 ||
            kind === PrimTypeKind.PRIMTYPE_u32 ||
            kind === PrimTypeKind.PRIMTYPE_i64 ||
            kind === PrimTypeKind.PRIMTYPE_u64 ||
            kind === PrimTypeKind.PRIMTYPE_c_int8_t ||
            kind === PrimTypeKind.PRIMTYPE_c_uint8_t ||
            kind === PrimTypeKind.PRIMTYPE_c_int16_t ||
            kind === PrimTypeKind.PRIMTYPE_c_uint16_t ||
            kind === PrimTypeKind.PRIMTYPE_c_int32_t ||
            kind === PrimTypeKind.PRIMTYPE_c_uint32_t ||
            kind === PrimTypeKind.PRIMTYPE_c_int64_t ||
            kind === PrimTypeKind.PRIMTYPE_c_uint64_t);
    }

    PrimType.isFloatingPointPrimTypeKind = function(kind) {
        return (kind === PrimTypeKind.PRIMTYPE_float ||
                kind === PrimTypeKind.PRIMTYPE_double ||
                kind === PrimTypeKind.PRIMTYPE_c_float ||
                kind === PrimTypeKind.PRIMTYPE_c_double ||
                kind === PrimTypeKind.PRIMTYPE_c_longdouble);
    }

    PrimType.prototype.getPrimTypeKind = function() { return this.getKind(); }

    PrimType.prototype.getByteSize = function() { return PrimType.getByteSizeOfPrimTypeKind(this.getPrimTypeKind()); }

    PrimType.prototype.isInteger = function() {
        return PrimType.isIntegerPrimTypeKind(this.getPrimTypeKind());
    }

    PrimType.prototype.isFloatingPoint = function() {
        return PrimType.isFloatingPointPrimTypeKind(getPrimTypeKind());
    }

    PrimType.prototype.isCType = function() {
        return this.getPrimTypeKind() >= PrimTypeKind.FIRST_C_PRIMTYPE;
    }

    PrimType.getBooleanType = function(world) {
        world.get
    }

    PrimType.getStringType = function(world) {
    }
    KIARA.PrimType = PrimType

    // -- World --

    var worldBuiltinTypes = [
        "i8", "u8", "i16", "u16", "i32", "u32", "i64", "u64", "float", "double", "boolean", "string",
        "c_int8_t", "c_uint8_t", "c_int16_t", "c_uint16_t", "c_int32_t", "c_uint32_t",
        "c_int64_t", "c_uint64_t",
        //"c_char", "c_wchar_t", "c_schar", "c_uchar", "c_short", "c_ushort",
        //"c_int", "c_uint", "c_long", "c_ulong", "c_longlong", "c_ulonglong",
        //"c_size_t", "c_ssize_t",
        "c_float", "c_double", "c_longdouble",
        "js_number", "js_string", "js_boolean"
    ];
    var worldTypes = worldBuiltinTypes;

    function World() {
        this.objects = {};
        this.namespace = new Namespace(this, "kiara");

        this.i8 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_i8));
        this.u8 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_u8));
        this.i16 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_i16));
        this.u16 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_u16));
        this.i32 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_i32));
        this.u32 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_u32));
        this.i64 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_i64));
        this.u64 = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_u64));
        this.float = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_float));
        this.double = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_double));
        this.boolean = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_boolean));
        this.string = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_string));

        this.c_int8_t   = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_int8_t));
        this.c_uint8_t  = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_uint8_t));
        this.c_int16_t  = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_int16_t));
        this.c_uint16_t = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_uint16_t));
        this.c_int32_t  = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_int32_t));
        this.c_uint32_t = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_uint32_t));
        this.c_int64_t  = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_int64_t));
        this.c_uint64_t = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_uint64_t));
        this.c_float    = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_float));
        this.c_double   = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_double));
        this.c_longdouble = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_c_longdouble));

        this.js_number = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_js_number));
        this.js_string = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_js_string));
        this.js_boolean = this.find(new PrimType(this, PrimTypeKind.PRIMTYPE_js_boolean));

        for (var i = 0; i < worldBuiltinTypes.length; ++i) {
            var tn = worldBuiltinTypes[i];
            this.namespace.bindType(tn, this[tn]);
        }
    }
    World.prototype.builtinTypeNames = worldBuiltinTypes;
    World.prototype.typeNames = worldTypes;

    function checkWorld(world) {
        if (!world || !(world instanceof World))
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "No KIARA world passed, use KIARA.World()");
    }

    World.prototype.getWorldNamespace = function() {
        return this.namespace;
    }

    World.prototype.createNamespace = function(name) {
        return new Namespace(this, name);
    }

    for (var i = 0; i < World.prototype.typeNames.length; ++i) {
        var tn = World.prototype.typeNames[i];
        World.prototype["type_"+tn] = new Function("return this."+tn+";");
    }

    World.prototype.findObject = function(object) {
        var hash = hashValue(object);
        if (this.objects.hasOwnProperty(hash)) {
            var values = this.objects[hash];
            for (var i = 0; i < values.length; ++i) {
                if (values[i] === object ||
                    (values[i] && values[i].equals && (values[i].equals(object))))
                    return values[i];
            }
            // handle hash collision
            values.push(object);
            return object;
        }
        this.objects[hash] = [object];
        return object;
    }
    World.prototype.find = World.prototype.findObject

    var globalWorld = new World();
    KIARA.World = function() { // singleton
        return globalWorld;
    }

    // -- PrimValueType --

    function PrimValueType(world, v) {
        var vtype;
        if (isNumber(v))
            vtype = "number";
        else if (isString(v))
            vtype = "string";
        else if (isBoolean(v))
            vtype = "boolean";
        else if (v === undefined || v === null)
            vtype = "null";
        else {
            vtype = "object";
        }
        Type.call(this, world, "value_type_"+vtype+"("+v+")", NodeKind.NODE_PRIMVALUETYPE, 0);
        this.value = v;
    }
    util.inherits(PrimValueType, Type);
    PrimValueType.prototype._className = "KIARA.PrimValueType";

    PrimValueType.get = function(world, value) {
        checkWorld(world);
        return world.find(new PrimValueType(world, value));
    }

    PrimValueType.prototype.getValue = function() { return this.value; }

    PrimValueType.prototype.equals = function(other) {
        if (!other || !PrimValueType.prototype.isPrototypeOf(other))
            return false;
        if (this.getKind() != other.getKind())
            return false;
        if (this.value !== other.getValue())
            return false;
        return true;
    }

    PrimValueType.prototype.hash = function() {
        var seed = 0;
        seed = hashCombine(seed, this.getKind());
        seed = hashCombine(seed, this.value);
        return seed;
    }

    KIARA.PrimValueType = PrimValueType;

    // -- VoidType --

    function VoidType(world) {
        Type.call(this, world, "void", NodeKind.NODE_VOIDTYPE, 0);
    }
    util.inherits(VoidType, Type);
    VoidType.prototype._className = "KIARA.VoidType";

    VoidType.get = function(world) {
        checkWorld(world);
        var void_ = world["void"];
        if (!void_)
            void_ = world["void"] = world.find(new VoidType(world));
        return void_;
    }

    KIARA.VoidType = VoidType;

    // -- TypeType --

    function TypeType(world) {
        Type.call(this, world, "type", NodeKind.NODE_TYPETYPE, 0);
    }
    util.inherits(TypeType, Type);
    TypeType.prototype._className = "KIARA.TypeType";

    TypeType.get = function(world) {
        checkWorld(world);
        var type = world.type;
        if (!type)
            type = world.type = world.find(new TypeType(world));
        return type;
    }

    KIARA.TypeType = TypeType;

    // -- UnresolvedSymbolType --

    function UnresolvedSymbolType(world) {
        Type.call(this, world, "unresolved_symbol", NodeKind.NODE_UNRESOLVEDSYMBOLTYPE, 0);
    }
    util.inherits(UnresolvedSymbolType, Type);
    UnresolvedSymbolType.prototype._className = "KIARA.UnresolvedSymbolType";

    UnresolvedSymbolType.get = function(world) {
        checkWorld(world);
        var unresolved_symbol = world.unresolved_symbol;
        if (!unresolved_symbol)
            unresolved_symbol = world.unresolved_symbol = world.find(new UnresolvedSymbolType(world));
        return unresolved_symbol;
    }

    KIARA.UnresolvedSymbolType = UnresolvedSymbolType;

    // -- AnyType --

    function AnyType(world) {
        Type.call(this, world, "any", NodeKind.NODE_ANYTYPE, 0);
    }
    util.inherits(AnyType, Type);
    AnyType.prototype._className = "KIARA.AnyType";

    AnyType.get = function(world) {
        checkWorld(world);
        var any = world.any;
        if (!any)
            any = world.any = world.find(new AnyType(world));
        return any;
    }

    KIARA.AnyType = AnyType;

    // -- StructType --

    function StructType(world, name, kind, elemsOrNum, options) {
        Type.call(this, world, name, kind, elemsOrNum);
        options = options || {};
        this.unique = !!options.unique;
        var names = options.names;

        this.elementDataList = new Array(this.getNumElements());
        if (isArray(names)) {
            if (names.length !== this.getNumElements())
                throw new KIARAError(KIARA.INVALID_ARGUMENT, "Number of elements ("+this.getNumElements()
                    +") is not equal to the number of names ("+names.length+")");
            this.setElementNames(names);
        }
    }
    util.inherits(StructType, Type);
    StructType.prototype._className = "KIARA.StructType";

    StructType.create = function(world, name, elemsOrNum) {
        return world.find(new StructType(world, name, NodeKind.NODE_STRUCTTYPE, elemsOrNum, {unique:true}));
    }
    StructType.get = function(world, name, elements, names) {
        var ty = new StructType(world, name, NodeKind.NODE_STRUCTTYPE, elements, {unique:false});
        ty.setElementNames(names);
        return world.find(ty);
    }

    StructType.prototype.isUnique = function() { return this.unique; }

    StructType.prototype.isOpaque = function() { return this.unique && this.getNumElements() === 0; }

    StructType.prototype.makeOpaque = function() {
        this.resizeElements(0);
    }

    StructType.prototype.resizeElements = function(newSize) {
        if (!this.isUnique())
            throw new KIARAError(KIARA.INVALID_OPERATION, "Structure is not unique");
        Type.prototype.resizeElements.call(this, newSize);
        this.elementDataList.length = newSize;
    }

    StructType.prototype.setElements = function(elems) {
        if (elems.length !== this.getNumElements())
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Number of passed elements is not equal to number of type elements");
        for (var i = 0; i < elems.length; ++i) {
            this.elements[i] = elems[i];
        }
    }

    StructType.prototype.setElementAt = function(index, element) {
        if (index < 0 || index >= this.getNumElements())
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Index out of bounds");
        if (!this.isUnique())
            throw new KIARAError(KIARA.INVALID_OPERATION, "Structure is not unique");
        this.elements[index] = element;
    }

    StructType.prototype._setElementAt = function(index, element) {
        if (index < 0 || index >= this.getNumElements())
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Index out of bounds");
        this.elements[index] = element;
    }

    StructType.prototype.getElementDataAt = function(index) {
        if (index < 0 || index >= this.getNumElements())
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Index out of bounds");
        return this.elementDataList[index];
    }

    StructType.prototype.setElementNames = function(names) {
        if (this.getNumElements() === 0 && (names === undefined || names === null))
            return;
        if (this.getNumElements() !== names.length)
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Number of names is not equal to number of elements");
        this.elementDataList.length = names.length;
        for (var i = 0; i < names.length; ++i) {
            this.setElementNameAt(i, names[i]);
        }
    }

    StructType.prototype.getElementNames = function() {
        var names = [];
        for (var i = 0; i < this.elementDataList.length; ++i) {
            names.push(this.elementDataList[i].name);
        }
        return names;
    }

    StructType.prototype.getElementNameAt = function(index) {
        return this.elementDataList[index].name || "";
    }

    StructType.prototype.setElementNameAt = function(index, name) {
        if (!this.elementDataList[index])
            this.elementDataList[index] = {name : name};
        else
            this.elementDataList[index].name = name;
    }

    StructType.prototype.toStringInner = function() {
        var s = "";
       if (!this.isOpaque()) {
            s += "{ ";
            var numElements = this.getNumElements();
            for (var i = 0; i < numElements; ++i) {
                var elem = this.getElementAt(i);
                var edata = this.getElementDataAt(i);
                if (edata && edata.name)
                    s += edata.name + " : ";
                if (elem)
                    s += elem.toString();
                else
                    s += "NULL";

                if (i != numElements-1)
                    s += ", ";
            }
            s += " }";
        }
        return s;
    }
    StructType.prototype.dumpInner = function() {
        console.log(this.toStringInner());
    }
    StructType.prototype.toString = function() {
        var s = "struct";
        var tn = this.getTypeName();
        if (tn)
            s += " "+tn;
        if (!this.isOpaque())
            s += " ";
        s += this.toStringInner();
        return s;
    }

    StructType.prototype.hash = function() {
        if (this.isUnique())
            return KIARAObject.prototype.hash.call(this);
        else
            return Type.prototype.hash.call(this);
    }

    StructType.prototype.equals = function(other) {
        if (this.isUnique())
            return this === other;
        else
            return Type.prototype.equals.call(this, other);
    }

    KIARA.StructType = StructType;

    // -- VariantType --

    function VariantType(world, elementTypes) {
        StructType.call(this, world, "variant", NodeKind.NODE_VARIANTTYPE, elementTypes.length, {unique: false});
        this.setElements(elementTypes);
    }
    util.inherits(VariantType, StructType);
    VariantType.prototype._className = "KIARA.VariantType";

    VariantType.get = function(world, elementTypes) {
        return world.find(new VariantType(world, elementTypes));
    }

    VariantType.prototype.getElementTypes = function() {
        return this.getElements();
    }

    VariantType.prototype.toString = function() {
        return "variant "+this.toStringInner();
    }

    KIARA.VariantType = VariantType;

    // -- ArrayType --

    function ArrayType(world, elementType) {
        StructType.call(this, world, "array", NodeKind.NODE_ARRAYTYPE, 1, {unique: false});
        if (!elementType)
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "elementType cannot be null");
        this._setElementAt(0, elementType); // internal set element
        //this.setElementNameAt(0, "Element");
    }
    util.inherits(ArrayType, StructType);
    ArrayType.prototype._className = "KIARA.ArrayType";

    ArrayType.get = function(world, elementType) {
        return world.find(new ArrayType(world, elementType));
    }

    ArrayType.prototype.getElementType = function() {
        return this.getElementAt(0);
    }

    ArrayType.prototype.toString = function() {
        return "array "+this.toStringInner();
    }

    KIARA.ArrayType = ArrayType;

    // -- FixedArrayType --

    function FixedArrayType(world, elementType, numElements) {
        StructType.call(this, world, "fixedArray", NodeKind.NODE_FIXEDARRAYTYPE, 2, {unique: false});
        if (!elementType)
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "elementType cannot be null");
        this.setElements([elementType, PrimValueType.get(world, numElements)]);
        this.setElementNames(["Element", "Size"]);
    }
    util.inherits(FixedArrayType, StructType);
    FixedArrayType.prototype._className = "KIARA.FixedArrayType";

    FixedArrayType.get = function(world, elementType, numElements) {
        return world.find(new FixedArrayType(world, elementType, numElements));
    }

    FixedArrayType.prototype.getElementType = function() {
        return this.getElementAt(0);
    }

    FixedArrayType.prototype.getArraySize = function() {
        return this.getElementAt(1).getValue();
    }

    FixedArrayType.prototype.toString = function() {
        return "fixedArray "+this.toStringInner();
    }

    KIARA.FixedArrayType = FixedArrayType;

    // -- Initialization --

    KIARA.init = function() { }
    KIARA.finalize = function() { }

    // -- Listener Support --

    // from http://stackoverflow.com/questions/10978311/implementing-events-in-my-own-object
    function augmentWithListener(object) {
        var _this = object;
        _this._events = {};

        _this.addListener = function(name, handler) {
            if (_this._events.hasOwnProperty(name))
                _this._events[name].push(handler);
            else
                _this._events[name] = [handler];
            if (_this._listenerAdded)
                _this._listenerAdded(name, handler);
            return _this;
        };

        _this.on = _this.addListener;

        _this.removeListener = function(name, handler) {
            /* This is a bit tricky, because how would you identify functions?
             This simple solution should work if you pass THE SAME handler. */
            if (!_this._events.hasOwnProperty(name))
                return;

            var index = _this._events[name].indexOf(handler);
            if (index != -1) {
                _this._events[name].splice(index, 1);
                if (_this._listenerRemoved)
                    _this._listenerRemoved(name, handler);
            }
        };

        _this.hasListeners = function(name) {
            if (!_this._events.hasOwnProperty(name))
                return false;
            return _this._events[name].length > 0;
        }

        _this.listeners = function(name) {
            if (!_this._events.hasOwnProperty(name))
                return [];
            return _this._events[name];
        }

        _this.emit = function(name) {
            if (!_this._events.hasOwnProperty(name))
                return;

            var args = Array.prototype.slice.call(arguments);
            args.splice(0, 1);

            var evs = _this._events[name], l = evs.length;
            for (var i = 0; i < l; i++) {
                evs[i].apply(null, args);
            }
        };
    }

    // -- Context --

    function Context() {
        augmentWithListener(this);
    }

    KIARA.createContext = function() { return new Context; }

    Context.prototype._handleError = function(error) {
        if (this.hasListeners('error'))
            return this.emit('error', error);
        throw error;
    }

    function checkContext(context) {
        if (!context || !(context instanceof Context))
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "No KIARA context passed, use KIARA.createContext");
    }

    function checkCallback(callback) {
        if (!callback)
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "No callback passed to the asynchronous function");
    }

    // -- MethodDescriptor --

    function MethodDescriptor(methodName, parsedTypeMapping, oneway) {
        this.methodName = methodName;
        this.parsedTypeMapping = parsedTypeMapping;
        this.isOneWay = (oneway == true);  // null == false
    }

    function checkMethodDescriptor(methodDescriptor) {
        if (!methodDescriptor || !(methodDescriptor instanceof MethodDescriptor))
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "No method descriptor passed");
    }

    // -- CallResponse --

    function CallResponse(connection, methodDescriptor, options) {
        this._connection = connection;
        this._methodDescriptor = methodDescriptor;
        this._result = null;
        this._resultType = null; // 'success' or 'error'
        augmentWithListener(this);
        var options = options || { };
        if (options.onerror)
            this.addListener('error', options.onerror);
        if (options.onresult)
            this.addListener('result', options.onresult);
    }

    CallResponse.prototype.getMethodName = function() {
        return this._methodDescriptor.methodName;
    }

    CallResponse.prototype.isOneWay = function() {
        return this._methodDescriptor.isOneWay;
    }

    CallResponse.prototype.setResult = function(result, resultType) {
        if (resultType != 'result' && resultType != 'error' && resultType != 'exception')
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "Unsupported result type: "+resultType);
        this._result = result;
        this._resultType = resultType;
        this._handleResult();
    }

    CallResponse.prototype._listenerAdded = function(name, handler) {
        if (this._error || this._result)
            this._handleResult();
    }

    CallResponse.prototype._handleResult = function() {
        if (this._resultType) {
            if (this._resultType == 'result' && this.hasListeners('result'))
                this.emit.apply(this, ['result', null, this._result]);

            if (this._resultType == 'exception' && this.hasListeners('result'))
                this.emit.apply(this, ['result', this._result]);

            if (this._resultType == 'exception' && this.hasListeners('failure'))
                this.emit.apply(this, ['failure', this._result]);

            if (this._resultType == 'error' && this.hasListeners('failure'))
                this.emit.apply(this, ['failure', null, this._result]);

            if (this._resultType == 'result' && this.hasListeners('success'))
                this.emit.apply(this, ['success', this._result]);

            if (this._resultType == 'exception' && this.hasListeners('exception'))
                this.emit.apply(this, ['exception', this._result]);

            if (this._resultType == 'error' && this.hasListeners('error'))
                this.emit.apply(this, ['error', this._result]);

            this._result = null;
            this._resultType = null;
        }
    }

    // -- Protocol base class --

    function Protocol(name) {
        this.name = name;
    }
    Protocol._protocols = {};

    Protocol.prototype.createMethodDescriptor = function(methodName, parsedTypeMapping, oneway) {
        return new MethodDescriptor(methodName, parsedTypeMapping, oneway);
    }
    Protocol.prototype.callMethod = function(callResponse, args) {
        throw new KIARAError(KIARA.UNSUPPORTED_FEATURE, "Protocol '"+this.name+"' not implemented");
    }
    Protocol.prototype.generateFuncWrapper = function(connection, methodDescriptor, options) {
        // TODO(rryk): Add support for [Oneway] calls.
        checkMethodDescriptor(methodDescriptor);
        var that = this;
        return function() {
            var callResponse = new CallResponse(connection, methodDescriptor, options);
            that.callMethod(callResponse, arguments);
            return callResponse;
        }
    }
    Protocol.prototype.registerFunc = function(methodDescriptor, nativeMethod) {
        throw new KIARAError(KIARA.UNSUPPORTED_FEATURE, "Protocol '"+this.name+"' not implemented");
    }

    function registerProtocol(name, protocolCtor) {
        if (typeof protocolCtor !== 'function')
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "registerProtocol require constructor function as argument");
        Protocol._protocols[name] = protocolCtor;
    }
    function getProtocol(name) {
        return Protocol._protocols[name];
    }
    KIARA.Protocol = Protocol;
    KIARA.registerProtocol = registerProtocol;
    KIARA.getProtocol = getProtocol;

    // -- Connection --

    function Connection(context, configURL, userCallback) {
        checkContext(context);
        this._context = context;
        this._url = null;
        this._errors = [];
        this._protocol = null;
        augmentWithListener(this);
        if (configURL)
            this.loadConfig(configURL, userCallback);
    }

    Connection.prototype._handleError = function(error) {
        if (this.hasListeners('error'))
            return this.emit('error', error);
        if (this._context)
            return this._context._handleError(error);
        throw error;
    }

    Connection.prototype._parseTypeMapping = function(qualifiedMethodName, typeMapping) {
        // TODO
        return { };
    }

    Connection.prototype.getProtocol = function() {
        return this._protocol;
    }

    // Constructs a function wrapper that will automatically serialize the method call and send it to the server.
    // The function will return an empty object, which can be used to set up a callback when response is received:
    //
    //   var login = conn.generateFuncWrapper(...);
    //   login(args).on('return', function(returnValue) { /* process return value here */ });
    //
    // Callback will be called with one argument that corresponds to the return value of the function in IDL.
    //
    // options argument is optional and can contain callback mappings:
    // result : function (result, exception) { ... }
    // error : function (error) { ... }
    //
    // result is invoked when either result or exception returned from the remote side
    // error is invoked when call failed
    // when both result and exception callbacks are defined, result is not called on
    // remote exception.
    Connection.prototype.generateFuncWrapper = function(qualifiedMethodName, typeMapping, options) {
        if (!this._protocol)
            throw new KIARAError(KIARA.INVALID_OPERATION, "Function wrapper cannot be generated before protocol is known, establish connection first");

        var parsedMapping = this._parseTypeMapping(qualifiedMethodName, typeMapping);
        var methodDescriptor = this._protocol.createMethodDescriptor(
            qualifiedMethodName, parsedMapping, this._isOneWay(qualifiedMethodName));
        return this._protocol.generateFuncWrapper(this, methodDescriptor, options);
    }

    Connection.prototype.registerFuncImplementation = function(qualifiedMethodName, typeMapping, nativeMethod) {
        if (!this._protocol)
            throw new KIARAError(KIARA.INVALID_OPERATION, "Native method cannot be registered before protocol is known, establish connection first");

        var parsedMapping = this._parseTypeMapping(qualifiedMethodName, typeMapping);
        var methodDescriptor = this._protocol.createMethodDescriptor(
            qualifiedMethodName, parsedMapping, this._isOneWay(qualifiedMethodName));
        return this._protocol.registerFunc(methodDescriptor, nativeMethod);
    }

    Connection.prototype.loadIDL = function(url, userCallback) {
        loadData(url,
            this._onIDLLoaded.bind(this, userCallback, url),
            this._onIDLLoadError.bind(this, userCallback, url));
    }

    Connection.prototype.loadConfig = function(url, userCallback) {
        loadData(url,
            this._onConfigLoaded.bind(this, userCallback, url),
            this._onIDLLoadError.bind(this, userCallback, url));
    }

    Connection.prototype._isOneWay = function(qualifiedMethodName) {
        var onewayMethods = [
            "omp.connectClient.handshake",
            "omp.connectInit.useCircuitCode",
            "omp.connectServer.handshakeReply",
            "omp.objectSync.updateObject",
            "omp.objectSync.deleteObject",
            "omp.objectSync.locationUpdate",
            "omp.movement.updateAvatarLocation",
            "omp.movement.updateAvatarMovement",
            "omp.chatServer.messageFromClient",
            "omp.chatClient.messageFromServer",
            "omp.animationServer.startAnimation",
            "objectsync.receiveObjectUpdates"
        ];
        return onewayMethods.indexOf(qualifiedMethodName) != -1;
    }

    Connection.prototype._onIDLLoaded = function(userCallback, url, response) {
        this._idls = this._idls || {};
        this._idls[url] = response;
        // TODO parse IDL, register types

        if (userCallback)
            userCallback(null, this);
    }

    Connection.prototype._onConfigLoaded = function(userCallback, url, response) {
        var that = this;
        function handleError(error) {
            if (userCallback)
                return userCallback(error);
            else
                return that._handleError(error);
        }

        if (!response || (!response.idlURL && !response.idlContents) ||
            !response.servers || response.servers.length == 0)
            handleError(new KIARAError(KIARA.INIT_ERROR, "Configuration file '" + url + "' is invalid."));

        var protocolCtor;
        var protocolConfig;

        // Try to use the server specified in the fragment.
        var fragmentIndex = url.indexOf("#");
        if (fragmentIndex != -1) {
            var index = parseInt(url.substring(fragmentIndex + 1));
            if (index >= 0 && index < response.servers.length && response.servers[index].protocol) {
                protocolCtor = getProtocol(response.servers[index].protocol.name);
                protocolConfig = response.servers[index].protocol;
            }
        }

        // Try to select any server that uses a supported protocol.
        if (!protocolCtor) {
            for (var i in response.servers) {
                if (!response.servers[i].protocol)
                    continue;

                protocolCtor = getProtocol(response.servers[i].protocol.name);
                protocolConfig = response.servers[i].protocol;

                if (!protocolCtor)
                    continue;
            }
        }

        // If we haven't found any server using a supported protocol, we must fail.
        if (!protocolCtor) {
            handleError(new KIARAError(KIARA.UNSUPPORTED_FEATURE,
                "Protocol '" + response.servers[i].protocol.name + "' is not supported"));
        }

        try {
            this._protocol = new protocolCtor(protocolConfig);
        } catch (e) {
            handleError(e);
        }

        if (response.idlURL)
            this.loadIDL(response.idlURL, userCallback);
        else if (response.idlContents)
            this._onIDLLoaded(userCallback, response.idlContents, response.idlContents)
    }

    Connection.prototype._onIDLLoadError = function(userCallback, xhr) {
        var error = new KIARAError(KIARA.IDL_LOAD_ERROR,
            "Could not load IDL '" + xhr._url +"': " + xhr.status + " - " + xhr.statusText);
        if (userCallback)
            userCallback(error);
        else
            this._handleError(error);
    }

    Context.prototype.openConnection = function(url, userCallback) {
        checkCallback(userCallback);
        return new Connection(this, url, userCallback);
    };

    // -- Service (For server only) --

    function Service(context, name, url) {
        checkContext(context);
        this._name = name;
        this._context = context;
        this._url = null;
        this._errors = [];
        this._methods = {};
        augmentWithListener(this);
        // TODO get IDL via URL
        //if (url)
        //    this.loadIDL(url, userCallback);
    }

    function checkService(service) {
        if (!service || !(service instanceof Service))
            throw new KIARAError(KIARA.INVALID_ARGUMENT, "No KIARA service passed, use context.createService");
    }

    Service.prototype._handleError = function(error) {
        if (this.hasListeners('error'))
            return this.emit('error', error);
        if (this._context)
            return this._context._handleError(error);
        throw error;
    }

    Service.prototype.getName = function() { return this._name; }

    Service.prototype._parseTypeMapping = function(qualifiedMethodName, typeMapping) {
        // TODO
        return { };
    }

    Service.prototype.registerMethod = function(qualifiedMethodName, typeMapping, func) {
        var parsedMapping = this._parseTypeMapping(qualifiedMethodName, typeMapping);
        this._methods[qualifiedMethodName] = {
            'parsedMapping' : parsedMapping,
            'method' : func
        };
    }

    Service.prototype.removeMethod = function(qualifiedMethodName) {
        if (this._methods.hasOwnProperty(qualifiedMethodName))
            delete this._methods[qualifiedMethodName];
    }

    Context.prototype.createService = function(name, url) {
        return new Service(this, url);
    };

    // -- Nodejs / Expressjs Middleware --

    var node = {
        _initialized : false,
        _protocols : {}
    };

    node.init = function() {
        if (node._initialized)
            return;
        if (!isNode)
            throw new KIARAError(KIARA.INVALID_OPERATION, "No nodejs environment detected");
        node._initialized = true;

        node.parse = require('url').parse;
        node.fs = require('fs');
        node.path = require('path');
        node.http = require('http');

        // from connect/lib/utils.js
        node.parseUrl = function(req) {
            var parsed = req._parsedUrl;
            if (parsed && parsed.href == req.url) {
                return parsed;
            } else {
                return req._parsedUrl = node.parse(req.url);
            }
        };

        // from express/lib/utils.js
        node.pathRegexp = function(path, keys, sensitive, strict) {
            if (path instanceof RegExp) return path;
            if (Array.isArray(path)) path = '(' + path.join('|') + ')';
            path = path
                .concat(strict ? '' : '/?')
                .replace(/\/\(/g, '(?:/')
                .replace(/(\/)?(\.)?:(\w+)(?:(\(.*?\)))?(\?)?(\*)?/g, function(_, slash, format, key, capture, optional, star){
                    keys.push({ name: key, optional: !! optional });
                    slash = slash || '';
                    return ''
                        + (optional ? '' : slash)
                        + '(?:'
                        + (optional ? slash : '')
                        + (format || '') + (capture || (format && '([^/.]+?)' || '([^/]+?)')) + ')'
                        + (optional || '')
                        + (star ? '(/*)?' : '');
                })
                .replace(/([\/.])/g, '\\$1')
                .replace(/\*/g, '(.*)');
            return new RegExp('^' + path + '$', sensitive ? '' : 'i');
        };

        node.writeError = function(res, status, errorMsg) {
            res.writeHead(status, node.http.STATUS_CODES[status],
                {"Content-Type": "text/plain"});
            if (errorMsg)
                res.write(errorMsg);
            res.end();
        }

        // init protocols

        //??? BEGIN PROTOCOL SPECIFIC PART
        try {
            node._protocols['jsonrpc'] = require('./connect-jsonrpc.js').serve;
        } catch (e) {
            console.warn('Could not load JSON-RPC protocol: ', e);
        }

        try {
            node._protocols['xmlrpc'] = require('./connect-xmlrpc.js').serve;
        } catch (e) {
            console.warn('Could not load XML-RPC protocol: ', e);
        }
        //??? END PROTOCOL SPECIFIC PART
    }

    node.middleware = function() {
        node.init();
        function app(req, res) { app.handle(req, res); }
        app.requestHandlers = [];
        app.handle = function(req, res) {
            var processNext;
            function next() {
                processNext = true;
            }
            for (var i in app.requestHandlers) {
                processNext = false;
                app.requestHandlers[i](req, res, next);
                if (!processNext)
                    break;
            }
            if (processNext) {
                node.writeError(res, 404, "Cannot "+req.method+" "+req.url);
            }
        }
        app.use = function(requestHandler) {
            if (requestHandler)
                app.requestHandlers.push(requestHandler);
        }
        return app;
    }

    node.serve = function (path, protocol, service, options) {
        node.init();
        if (!path)
            throw new KIARAError(KIARA.INVALID_ARGUMENT, 'KIARA.node.serve() require path');
        checkService(service);

        var options = options || {};
        var keys = [];
        var sensitive = true;
        var regexp = node.pathRegexp(path
            , keys
            , options.sensitive
            , options.strict);

        var protocolHandler = node._protocols[protocol];
        if (!protocolHandler)
            throw new KIARAError(KIARA.UNSUPPORTED_FEATURE, "Protocol '"+protocol+"' is not supported by KIARA");

        var methodTable = {};
        for (var name in service._methods) {
            methodTable[name] = service._methods[name].method;
        }
        var serveProtocol = protocolHandler(methodTable);

        return function(req, res, next) {
            var path = node.parseUrl(req).pathname;
            var m = regexp.exec(path);
            if (!m)
                return next();
            console.log("KIARA RPC Handler: for "+path);
            //res.end("KIARA RPC Handler: for "+path);
            return serveProtocol(req, res, next);
        }
    }

    node.serveFiles = function(path, root, options) {
        node.init();

        var options = options || {};
        var keys = [];
        var sensitive = true;
        var regexp = node.pathRegexp(path
            , keys
            , options.sensitive
            , options.strict);

        root = node.path.normalize(root);
        return function(req, res, next) {
            var reqPath = node.parseUrl(req).pathname;
            var m = regexp.exec(reqPath);
            if (!m)
                return next();

            if (root) {
                reqPath = node.path.normalize(node.path.join(root, reqPath));
                if (reqPath.indexOf(root) != 0)
                    return node.writeError(res, 403);
            } else {
                if (~reqPath.indexOf('..')) {
                    return node.writeError(res, 403);
                }
            }

            node.fs.exists(reqPath, function(exists) {
                if (!exists) {
                    return node.writeError(res, 404, "404 Not Found\n");
                } else {
                    node.fs.readFile(reqPath, function(err, file) {
                        if (err) {
                            return node.writeError(res, 500, err+"\n");
                        } else {
                            res.writeHeader(200);
                            res.write(file, "binary");
                            res.end();
                        }
                    });
                }
            });
        }
    }

    KIARA.node = node;
    KIARA.CallResponse = CallResponse;

    return KIARA;
});
