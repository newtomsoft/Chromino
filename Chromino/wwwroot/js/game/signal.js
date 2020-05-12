﻿var ConnectionHubGame;
function CallSignalR(guid) {
    ConnectionHubGame = new signalR.HubConnectionBuilder().withUrl("/hubGame").withAutomaticReconnect().build();
    ConnectionHubGame.start()
        .then(function () { if (guid !== undefined) SendAddToGame(); })
        .catch(function (err) {
            return console.error(err.toString());
        });

    ConnectionHubGame.on("ReceivePlayersLogged", function (newPlayersId) {
        ReceivePlayersLogged(newPlayersId);
    });

    ConnectionHubGame.on("ReceivePlayersInGame", function (newPlayersId) {
        ReceivePlayersInGame(newPlayersId);
    });

    ConnectionHubGame.on("ReceiveChatMessageSent", function (guid) {
        ReceiveChatMessageSent(guid);
    });

    ConnectionHubGame.on("ReceivePrivateMessageMessageSent", function (senderId) {
        ReceivePrivateMessageMessageSent(senderId);
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
