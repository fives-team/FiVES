function callMethod(plugin, method) {
    var parameterFields = $("input[id^='param-" + plugin + "-" + method + "']");
    var params = [];
    for (var i = 0; i < parameterFields.length; i++)
    {
        params.push(parameterFields[i].value);
    }
    var content = JSON.stringify(params);
    $.ajax({
        url: "/diagnosis/action/" + plugin + "/" + method,
        method: "POST",
        dataType: "json",
        contentType: "application/json",
        data: content
    });
}