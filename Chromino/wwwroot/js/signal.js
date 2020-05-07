var ConnectionHubGame;
function CallSignalR() {
    ConnectionHubGame = new signalR.HubConnectionBuilder().withUrl("/hubChat").withAutomaticReconnect().build();

    ConnectionHubGame.on("ReceiveMessageSent", function (gameId) {
        OpponentMessageSent(gameId);
    });

    ConnectionHubGame.on("ReceiveChrominoPlayed", function (gameId, chrominoPlayed) {
        OpponentChrominoPlayed(gameId, chrominoPlayed);
    });

    ConnectionHubGame.on("ReceiveTurnSkipped", function (gameId) {
        OpponentTurnSkipped(gameId);
    });

    ConnectionHubGame.on("ReceiveChrominoDrawn", function (gameId) {
        OpponentChrominoDrawn(gameId);
    });

    ConnectionHubGame.on("ReceiveBotChrominoPlayed", function (gameId, chrominoPlayed, isDrawn) {
        BotChrominoPlayed(gameId, chrominoPlayed, isDrawn);
    });

    ConnectionHubGame.on("ReceiveBotTurnSkipped", function (gameId, isDrawn) {
        BotTurnSkipped(gameId, isDrawn);
    });


    ConnectionHubGame.start().then(function () {
        $("#ButtonChat").show();
    }).catch(function (err) {
        return console.error(err.toString());
    });
}
