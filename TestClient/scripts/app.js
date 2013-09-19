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

    function login() {
        var login = $("#login").val();
        var password = $("#password").val();

        if (!FIVES.Communication.FivesCommunicator.auth(login, password)) {
            $("#login").val("");
            $("#password").val("");
            return false;
        }

        FIVES.Communication.FivesCommunicator.connect();
        return true;
    }

    function main() {
        var context = KIARA.createContext();
        var service = "kiara/fives.json";

        FIVES.Communication.FivesCommunicator.initialize(context, service);
        FIVES.Resources.SceneManager.initialize("xml3dView");

        $('#signin').modal();
        $("#signin").on("hide.bs.modal", login);
        $("#btn-signin").click(function() { $('#signin').modal("hide") });
    }
    $(document).ready(main);
});
