function callAction(plugin, method) {
    $.ajax({
        url: "/diagnosis/action/" + plugin + "/" + method,
        method: "POST"
    });
}