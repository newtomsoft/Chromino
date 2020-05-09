var ConnectionHubGame;
function CallSignalR() {
    ConnectionHubGame = new signalR.HubConnectionBuilder().withUrl("/hubGame").withAutomaticReconnect().build();
    ConnectionHubGame.start().then(function () {
        SendAddToGroup();
    }).catch(function (err) {
        return console.error(err.toString());
    });;

    ConnectionHubGame.on("ReceiveMessageSent", function (gameId) {
        OpponentMessageSent(gameId);
    });
}
