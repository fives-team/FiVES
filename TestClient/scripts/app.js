require.config({
    paths: {
        'jquery' : 'lib/jquery',
        'kiara' : 'lib/kiara',
        'websocket-json' : 'lib/websocket-json'
    }
});

requirejs(['kiara', 'jquery', 'websocket-json'],
function(KIARA, $) {

    function setScript(guid) {
        //var script = promptString("serverScript = ");
        var script = "console.log('hello from client');";
        createServerScriptFor(guid, script);
        return false;
    }

    function main() {
        var context = KIARA.createContext();
        var service = "kiara/fives.json";
        FIVES.Resources.SceneManager.initialize("xml3dView");
        FIVES.Communication.FivesCommunicator.initialize(context, service);
    }
    $(document).ready(main);
});
