function SendMessageSent() {
    ConnectionHubGame.invoke("SendMessageSent", GameId).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendChrominoPlayed(chrominoPlayed) {
    ConnectionHubGame.invoke("SendChrominoPlayed", GameId, chrominoPlayed).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendTurnSkipped() {
    ConnectionHubGame.invoke("SendTurnSkipped", GameId).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendChrominoDrawn() {
    ConnectionHubGame.invoke("SendChrominoDrawn", GameId).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendBotChrominoPlayed(chrominoPlayed, isDrawn) {
    ConnectionHubGame.invoke("SendBotChrominoPlayed", GameId, chrominoPlayed, isDrawn).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendBotTurnSkipped(isDrawn) {
    ConnectionHubGame.invoke("SendBotTurnSkipped", GameId, isDrawn).catch(function (err) {
        return console.error(err.toString());
    });
};