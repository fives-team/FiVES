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

    var loginComplete = false;
    function login() {
        if (loginComplete)
            return true;

        var login = $("#login").val();
        var password = $("#password").val();

        FIVES.Communication.FivesCommunicator.auth(login, password, function(success, sessionKey) {
            if (success) {
                FIVES.Communication.FivesCommunicator.connect(function() {
                    loginComplete = true;
                    $("singin").modal("hide");
                });
            } else {
                $("#login").val("");
                $("#password").val("");
                // TODO: show "Error: Failed to sign in.", enable inputs and button
            }
        });

        // TODO: show "Signing in...", disable inputs and button

        return false;
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
