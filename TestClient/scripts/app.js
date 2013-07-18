requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {
    function main() {
        var context = KIARA.createContext();
        var service = "http://localhost/projects/test-client/kiara/fives.json";
        var conn = context.openConnection(service, function() {
            var implements = conn.generateFuncWrapper("kiara.implements");
            implements(["clientsync"]).on("result", function(error, supported) {
               if (supported[0]) {
                   var listObjects = conn.generateFuncWrapper("clientsync.listObjects");
                   listObjects().on("result", function(error, objects) {
                       for (var i = 0; i < objects.length; i++) {
                           var div = document.createElement("div");
                           div.appendChild(document.createTextNode(objects[i]));
                           document.body.appendChild(div);
                       }
                   })
               }
            });
        });
    }

    $(document).ready(main);
});
