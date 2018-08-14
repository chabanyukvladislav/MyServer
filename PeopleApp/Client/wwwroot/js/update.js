"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/Update").build();
connection.on("del", function (value) {
    $(`#${value}`).remove();
});
connection.on("add", function (id, name, surname, phone) {
    $(`#TBody`).add(`<tr id="${id}"><td>${name}</td><td>${surname}</td><td>${phone}</td></tr>`);
});
connection.start();