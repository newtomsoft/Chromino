var ConnectionHubGame;
function CallSignalR() {
    ConnectionHubGame = new signalR.HubConnectionBuilder().withUrl("/hubGame").withAutomaticReconnect().build();
    ConnectionHubGame.start().then(function () {
        SendAddToGroup(Guid);
    }).catch(function (err) {
        return console.error(err.toString());
    });;

    ConnectionHubGame.on("ReceiveMessageSent", function (gameId) {
        OpponentMessageSent(gameId);
    });

    ConnectionHubGame.on("ReceiveChrominoPlayed", function (chrominoPlayed) {
        OpponentChrominoPlayed(chrominoPlayed);
    });

    ConnectionHubGame.on("ReceiveTurnSkipped", function () {
        OpponentTurnSkipped();
    });

    ConnectionHubGame.on("ReceiveChrominoDrawn", function () {
        OpponentChrominoDrawn();
    });

    ConnectionHubGame.on("ReceiveBotChrominoPlayed", function (chrominoPlayed, isDrawn) {
        BotChrominoPlayed(chrominoPlayed, isDrawn);
    });

    ConnectionHubGame.on("ReceiveBotTurnSkipped", function (isDrawn) {
        BotTurnSkipped(isDrawn);
    });
}
