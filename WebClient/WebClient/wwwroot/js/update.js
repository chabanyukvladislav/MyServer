let hubUrl = 'http://localhost:3668/update';
const hubConnection = new signalR.HubConnectionBuilder().withUrl(hubUrl).configureLogging(signalR.LogLevel.Information).build();
hubConnection.start();
hubConnection.on("Update", function () {
    location.reload();
});