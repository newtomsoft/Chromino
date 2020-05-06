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


// BOT

function SendBotChrominoPlayed(chrominoPlayed) {
    ConnectionHubGame.invoke("SendBotChrominoPlayed", GameId, chrominoPlayed).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendBotTurnSkipped() {
    ConnectionHubGame.invoke("SendBotTurnSkipped", GameId).catch(function (err) {
        return console.error(err.toString());
    });
};