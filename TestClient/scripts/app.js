requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {
    var listObjects;
    var getObjectPosition;
    var createEntityAt;

    function requestPosition(guid) {
        getObjectPosition(guid).on("result", function(error, position) {
           alert("Object " + guid + " is located at (" + position.x + ", " + position.y + ", " + position.z + ").");
        });
    }

    function promptFloat(msg) {
        var value;
        do {
            value = prompt(msg);
        } while (isNaN(parseFloat(value)) || !isFinite(value))
        return parseFloat(value);
    }

    function createNewEntity(conn) {
        var x = promptFloat("x = ");
        var y = promptFloat("y = ");
        var z = promptFloat("z = ");

        createEntityAt(x, y, z).on("result", function() {
            location.reload();
        });
    }

    function main() {
        context = KIARA.createContext();
        service = "kiara/fives.json";
        context.openConnection(service, function(error, conn) {
            var implements = conn.generateFuncWrapper("kiara.implements");
            implements(["clientsync"]).on("result", function(error, supported) {
               if (supported[0]) {
                   listObjects = conn.generateFuncWrapper("clientsync.listObjects");
                   getObjectPosition = conn.generateFuncWrapper("clientsync.getObjectPosition");
                   createEntityAt = conn.generateFuncWrapper("editing.createEntityAt");
                   listObjects().on("result", function(error, objects) {
                       for (var i = 0; i < objects.length; i++) {
                           var div = document.createElement("div");
                           var guid = objects[i];
                           div.appendChild(document.createTextNode(guid));
                           div.addEventListener("click", requestPosition.bind(null, guid));
                           div.setAttribute("style", "border: 1px solid black; background-color: gray; margin: 2px;");
                           document.body.appendChild(div);
                       }

                       var button = document.createElement("button");
                       button.appendChild(document.createTextNode("Create entity"));
                       button.addEventListener("click", createNewEntity.bind(conn));
                       document.body.appendChild(button);
                   })
               }
            });
        });
    }

    $(document).ready(main);
});
