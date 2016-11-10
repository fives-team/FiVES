function callMethod(plugin, method) {
    var parameters = $("#form-" + plugin + "-" + method);
    var data = JSON.stringify(parameters.serializeArray());
    $.ajax({
        url: "/diagnosis/action/" + plugin + "/" + method,
        method: "POST",
        dataType: "json",
        contentType: "application/json",
        content: data
    });
}