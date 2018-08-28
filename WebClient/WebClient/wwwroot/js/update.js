let hubUrl = 'http://185.247.21.82:8080/update';
const hubConnection = new signalR.HubConnectionBuilder().withUrl(hubUrl).configureLogging(signalR.LogLevel.Information).build();
hubConnection.start();
hubConnection.on("Update", function () {
    location.reload();
});