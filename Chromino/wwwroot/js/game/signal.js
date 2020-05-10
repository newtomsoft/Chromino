var ConnectionHubGame;
function CallSignalR() {
    ConnectionHubGame = new signalR.HubConnectionBuilder().withUrl("/hubGame").withAutomaticReconnect().build();
    ConnectionHubGame.start()
        .catch(function (err) {
            return console.error(err.toString());
        });

    ConnectionHubGame.on("ReceivePlayersInGame", function (newPlayersId) {
        ReceivePlayersInGame(newPlayersId);
    });

    ConnectionHubGame.on("ReceiveMessageSent", function (guid) {
        OpponentMessageSent(guid);
    });

    ConnectionHubGame.on("ReceiveChrominoPlayed", function (guid, chrominoPlayed) {
        OpponentChrominoPlayed(guid, chrominoPlayed);
    });

    ConnectionHubGame.on("ReceiveTurnSkipped", function (guid) {
        OpponentTurnSkipped(guid);
    });

    ConnectionHubGame.on("ReceiveChrominoDrawn", function (guid) {
        OpponentChrominoDrawn(guid);
    });

    ConnectionHubGame.on("ReceiveBotChrominoPlayed", function (guid, chrominoPlayed, isDrawn) {
        BotChrominoPlayed(guid, chrominoPlayed, isDrawn);
    });

    ConnectionHubGame.on("ReceiveBotTurnSkipped", function (guid, isDrawn) {
        BotTurnSkipped(guid, isDrawn);
    });
}
