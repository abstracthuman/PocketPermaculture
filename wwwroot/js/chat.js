"use strict";

var signalR = require('@aspnet/signalr');

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/Chat")
    .build();

connection.on("Send", function (message) {
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
});

document.getElementById("groupmsg").addEventListener("click", async (event) => {
    var userName = document.getElementById("userName").value;
    var groupName = document.getElementById("group-name").value;
    var groupMsg = document.getElementById("group-message-text").value;
    try {
        await connection.invoke("SendMessageToGroup", userName, groupName, groupMsg);
    }
    catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});

document.getElementById("join-group").addEventListener("click", async (event) => {
    var userName = document.getElementById("userName").value;
    var groupName = document.getElementById("group-name").value;
    try {
        await connection.invoke("AddToGroup", userName, groupName);
    }
    catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});

document.getElementById("leave-group").addEventListener("click", async (event) => {
    var userName = document.getElementById("userName").value;
    var groupName = document.getElementById("group-name").value;
    try {
        await connection.invoke("RemoveFromGroup", userName, groupName);
    }
    catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});

// We need an async function in order to use await, but we want this code to run immediately,
// so we use an "immediately-executed async function"
(async () => {
    try {
        await connection.start();
    }
    catch (e) {
        console.error(e.toString());
    }
})();
