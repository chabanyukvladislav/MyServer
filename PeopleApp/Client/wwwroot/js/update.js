"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/Update").build();
connection.on("update", function () {
    $(document).refresh();
});
connection.start();