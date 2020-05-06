function SendMessageSent() {
    ConnectionHubGame.invoke("SendMessageSent", GameId).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendChrominoPlayed(chrominoPlayed) {
    ConnectionHubGame.invoke("SendChrominoPlayed", GameId, PlayerName, chrominoPlayed).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendTurnSkipped() {
    ConnectionHubGame.invoke("SendTurnSkipped", GameId, PlayerName).catch(function (err) {
        return console.error(err.toString());
    });
};

function SendChrominoDrawn() {
    ConnectionHubGame.invoke("SendChrominoDrawn", GameId, PlayerName).catch(function (err) {
        return console.error(err.toString());
    });
};