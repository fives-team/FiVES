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

    function resetFormAndReportError(message) {
        // Reset the form.
        $("#signin-login").prop('disabled', false);
        $("#signin-password").prop('disabled', false);
        $("#signin-btn").button("reset");

        // Show error message.
        $("#signin-failed").text(message);
        $("#signin-failed").show();

        // Focus the login input.
        $("#signin-login").focus();
    }

    var loginComplete = false;
    var signinBtnPressed = false;
    function login() {
        // This is to allow hiding modal when login is complete.
        if (loginComplete)
            return true;

        // This is to ignore attempts to close the signin modal by clicking outside of it.
        if (!signinBtnPressed)
            return false;
        else
            signinBtnPressed = false; // reset the value

        var login = $("#signin-login").val();
        var password = $("#signin-password").val();

        var connectCallback = function(success, message) {
            if (success) {
                loginComplete = true;
                $("#signin-modal").modal("hide");
            } else {
                resetFormAndReportError(message);
            }
        };

        var authCallback = function(success, message) {
            if (success) {
                FIVES.Communication.FivesCommunicator.connect(connectCallback);
            } else {
                resetFormAndReportError(message);
            }
        };

        // Make a small pause so that users can appreciated the "Signing in..." message.
        setTimeout( function() {
            FIVES.Communication.FivesCommunicator.auth(login, password, authCallback);
        }, 1000);

        // Disable input fields and button, hide error message if any.
        $("#signin-btn").button("loading");
        $("#signin-login").prop('disabled', true);
        $("#signin-password").prop('disabled', true);
        $("#signin-failed").hide();

        return false;
    }

    function main() {
        var context = KIARA.createContext();
        var service = "kiara/fives.json";

        FIVES.Communication.FivesCommunicator.initialize(context, service);
        FIVES.Resources.SceneManager.initialize("xml3dView");

        // Show signin modal.
        $('#signin-modal').modal("show");
        $("#signin-modal").on("hide.bs.modal", login);
        $("#signin-btn").click(function() { signinBtnPressed = true; $('#signin-modal').modal("hide"); });
    }
    $(document).ready(main);
});
