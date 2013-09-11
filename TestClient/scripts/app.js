require.config({
    paths: {
        'jquery' : 'lib/jquery',
        'kiara' : 'lib/kiara',
        'websocket-json' : 'lib/websocket-json'
    }
});

requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {
    var listObjects;
    var getObjectLocation;
    var createEntityAt;
    var createServerScriptFor;
    var notifyAboutNewObjects;
    var getObjectMesh;
    var connection;

    function requestLocation(guid) {
        getObjectLocation(guid).on("result", function(error, loc) {
           alert("Object " + guid + " is located at pos: (" + loc.position.x + ", " + loc.position.y + ", " +
                 loc.position.z + ") and rot: (" + loc.orientation.x + ", " + loc.orientation.y + ", " +
                 loc.orientation.z + ", " + loc.orientation.w + ").");
        });
    }

    function requestMeshData(guid) {
        getObjectMesh(guid).on("result", function(error, mesh) {

        });
    }

    function promptFloat(msg) {
        var value;
        do {
            value = prompt(msg);
        } while (isNaN(parseFloat(value)) || !isFinite(value))
        return parseFloat(value);
    }

    function promptString(msg) {
        var value;
        do {
            value = prompt(msg);
        } while (value.toString() == "")
        return value.toString();
    }

    function createNewEntity() {
        var x = promptFloat("x = ");
        var y = promptFloat("y = ");
        var z = promptFloat("z = ");
        createEntityAt(x, y, z);
    }

    function setScript(guid) {
        //var script = promptString("serverScript = ");
        var script = "console.log('hello from client');";
        createServerScriptFor(guid, script);
        return false;
    }

    function addObjectButton(guid) {
        var div = document.createElement("div");
        div.appendChild(document.createTextNode(guid));
        div.addEventListener("click", requestLocation.bind(null, guid));
        //div.addEventListener("click", setScript.bind(null, guid));
        div.setAttribute("style", "border: 1px solid black; background-color: gray; margin: 2px;");
        document.body.appendChild(div);
    }

    function retrieveEntityData(guid) {
        var entityDocument = {};
        entityDocument.guid = guid;
        console.log("Adding new entity document with guid " + guid);
        getObjectLocation(guid).on("result", function(error, location) {
            entityDocument.location = {};
            entityDocument.location.position = location.position;
            entityDocument.location.orientation = location.orientation;
        });

        getObjectMesh(guid).on("result", function(error, mesh) {
            entityDocument.mesh = {};
            entityDocument.mesh.scale = mesh.scale;
            entityDocument.mesh.scale.x = 1;
            entityDocument.mesh.scale.y = 1;
            entityDocument.mesh.scale.z = 1;

            entityDocument.mesh.uri = mesh.uri || "/models/firetruck/xml3d/firetruck.xml";

            FIVES.Resources.SceneManager.addMeshForObject(entityDocument);
        })

    }

    var listObjectsCallback =  function(error, objects) {
        // Create a button for adding a new object.
        var createButton = document.createElement("button");
        createButton.appendChild(document.createTextNode("Create entity"));
        createButton.addEventListener("click", createNewEntity);
        document.body.appendChild(createButton);

        // Add existing objects.
        for (var i = 0; i < objects.length; i++)
            retrieveEntityData(objects[i]);

        // Listen for new objects.
        notifyAboutNewObjects(retrieveEntityData);
    };

    var clientSyncCallback = function(error, supported) {
        if (supported[0]) {
            listObjects = connection.generateFuncWrapper("clientsync.listObjects");
            getObjectLocation = connection.generateFuncWrapper("clientsync.getObjectLocation");
            createEntityAt = connection.generateFuncWrapper("editing.createEntityAt");
            createServerScriptFor = connection.generateFuncWrapper("scripting.createServerScriptFor");
            notifyAboutNewObjects = connection.generateFuncWrapper("clientsync.notifyAboutNewObjects");
            getObjectMesh = connection.generateFuncWrapper("clientsync.getObjectMesh");
            listObjects().on("result", listObjectsCallback);
        }
    };

    var callbackFunction = function(error, conn) {
            connection = conn;
            var implements = connection.generateFuncWrapper("kiara.implements");
            implements(["clientsync"]).on("result", clientSyncCallback);
    }

    function main() {
        context = KIARA.createContext();
        service = "kiara/fives.json";
        FIVES.Resources.SceneManager.initialize("xml3dView");
        context.openConnection(service, callbackFunction );
    }

    $(document).ready(main);
});
