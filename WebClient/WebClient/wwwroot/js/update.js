let hubUrl = 'http://192.168.1.19:1920/update';
const hubConnection = new signalR.HubConnectionBuilder().withUrl(hubUrl).configureLogging(signalR.LogLevel.Information).build();
hubConnection.start();
hubConnection.on("Update", function () {
    location.reload();
});