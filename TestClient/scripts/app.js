requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {
    var getObjectPosition;

    function requestPosition(guid) {
        getObjectPosition(guid).on("result", function(error, position) {
           alert("Object " + guid + " is located at (" + position.x + ", " + position.y + ", " + position.z + ").");
        });
    }

    function main() {
        context = KIARA.createContext();
        service = "http://localhost/projects/test-client/kiara/fives.json";
        conn = context.openConnection(service, function() {
            var implements = conn.generateFuncWrapper("kiara.implements");
            implements(["clientsync"]).on("result", function(error, supported) {
               if (supported[0]) {
                   var listObjects = conn.generateFuncWrapper("clientsync.listObjects");
                   getObjectPosition = conn.generateFuncWrapper("clientsync.getObjectPosition");
                   listObjects().on("result", function(error, objects) {
                       for (var i = 0; i < objects.length; i++) {
                           var div = document.createElement("div");
                           var guid = objects[i];
                           div.appendChild(document.createTextNode(guid));
                           div.addEventListener("click", function() { requestPosition(guid); })
                           div.setAttribute("style", "border: 1px solid black; background-color: gray; margin: 2px;");
                           document.body.appendChild(div);
                       }
                   })
               }
            });
        });
    }

    $(document).ready(main);
});
