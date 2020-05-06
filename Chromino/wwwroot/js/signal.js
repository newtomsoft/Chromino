var ConnectionHubGame;
function CallSignalR() {
    ConnectionHubGame = new signalR.HubConnectionBuilder().withUrl("/hubChat").withAutomaticReconnect().build();

    ConnectionHubGame.on("ReceiveMessageSent", function (gameId) {
        OpponentMessageSent(gameId);
    });

    ConnectionHubGame.on("ReceiveChrominoPlayed", function (gameId, playerName, chrominoPlayed) {
        OpponentChrominoPlayed(gameId, playerName, chrominoPlayed);
    });

    ConnectionHubGame.on("ReceiveTurnSkipped", function (gameId, playerName) {
        OpponentTurnSkipped(gameId, playerName);
    });

    ConnectionHubGame.on("ReceiveChrominoDrawn", function (gameId, playerName) {
        OpponentChrominoDrawn(gameId, playerName);
    });

    ConnectionHubGame.start().then(function () {
        $("#ButtonChat").show();
    }).catch(function (err) {
        return console.error(err.toString());
    });
}
