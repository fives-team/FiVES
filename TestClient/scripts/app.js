require.config({
    paths: {
        'jquery' : 'lib/jquery',
        'kiara' : 'lib/kiara',
        'websocket-json' : 'lib/websocket-json'
    }
});

requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {

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
        FIVES.Communication.FivesCommunicator.createEntityAt(x, y, z);
    }

    function setScript(guid) {
        //var script = promptString("serverScript = ");
        var script = "console.log('hello from client');";
        createServerScriptFor(guid, script);
        return false;
    }

    function addButton() {
        var createButton = document.createElement("button");
        createButton.appendChild(document.createTextNode("Create entity"));
        createButton.addEventListener("click", createNewEntity);
        document.body.appendChild(createButton);
    }

    function main() {
        var context = KIARA.createContext();
        var service = "kiara/fives.json";
        FIVES.Resources.SceneManager.initialize("xml3dView");
        FIVES.Communication.FivesCommunicator.initialize(context, service);
    }
    $(document).ready(main);
});
