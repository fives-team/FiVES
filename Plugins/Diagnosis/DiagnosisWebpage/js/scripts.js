function callMethod(plugin, method) {
    $("div[id^='success-response']").hide();
    $("div[id^='error-response']").hide();
    var parameterFields = $("input[id^='param-" + plugin + "-" + method + "']");
    var params = [];
    for (var i = 0; i < parameterFields.length; i++) {
        params.push(parameterFields[i].value);
    }
    var content = JSON.stringify(params);
    $.ajax({
        url: "/diagnosis/action/" + plugin + "/" + method,
        method: "POST",
        contentType: "application/json",
        data: content
    })
    .done(function (data, status) { callSuccess(plugin, method, data, status); })
    .fail(function (data, status) { callError(plugin, method, data, status); });
}

function callSuccess(plugin, method) {
    $("#success-response-" + plugin + "-" + method).show();
}

function callError(plugin, method, data, status) {
    var responseField = $("#error-response-" + plugin + "-" + method);
    responseField.text(data.responseText);
    responseField.show();
}