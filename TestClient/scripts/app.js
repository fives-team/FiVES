requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {
    var listObjects;
    var getObjectLocation;
    var createEntityAt;
    var createServerScriptFor;
    var notifyAboutNewObjects;

    function requestLocation(guid) {
        getObjectLocation(guid).on("result", function(error, loc) {
           alert("Object " + guid + " is located at pos: (" + loc.position.x + ", " + loc.position.y + ", " +
                 loc.position.z + ") and rot: (" + loc.orientation.x + ", " + loc.orientation.y + ", " +
                 loc.orientation.z + ", " + loc.orientation.w + ").");
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

    function createNewEntity(conn) {
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

    function main() {
        context = KIARA.createContext();
        service = "kiara/fives.json";
        context.openConnection(service, function(error, conn) {
            var implements = conn.generateFuncWrapper("kiara.implements");
            implements(["clientsync"]).on("result", function(error, supported) {
               if (supported[0]) {
                   listObjects = conn.generateFuncWrapper("clientsync.listObjects");
                   getObjectLocation = conn.generateFuncWrapper("clientsync.getObjectLocation");
                   createEntityAt = conn.generateFuncWrapper("editing.createEntityAt");
                   createServerScriptFor = conn.generateFuncWrapper("scripting.createServerScriptFor");
                   notifyAboutNewObjects = conn.generateFuncWrapper("clientsync.notifyAboutNewObjects");
                   listObjects().on("result", function(error, objects) {
                       var createButton = document.createElement("button");
                       createButton.appendChild(document.createTextNode("Create entity"));
                       createButton.addEventListener("click", createNewEntity.bind(null, conn));
                       document.body.appendChild(createButton);

                       var createButton = document.createElement("button");
                       createButton.appendChild(document.createTextNode("Start listening for new objects"));
                       createButton.addEventListener("click", function() { notifyAboutNewObjects(addObjectButton) });
                       document.body.appendChild(createButton);

                       for (var i = 0; i < objects.length; i++)
                           addObjectButton(objects[i]);
                   })
               }
            });
        });
    }

    $(document).ready(main);
});
