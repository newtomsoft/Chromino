function SendAddToGroup() {
    ConnectionHubGame.invoke("AddToGroup", Guid).catch(function (err) { return console.error(err.toString()); });
};

function SendMessageSent() {
    ConnectionHubGame.invoke("SendMessageSent", Guid).catch(function (err) { return console.error(err.toString()); });
};

function SendChrominoPlayed(chrominoPlayed) {
    ConnectionHubGame.invoke("SendChrominoPlayed", Guid, chrominoPlayed).catch(function (err) { return console.error(err.toString()); });
};

function SendTurnSkipped() {
    ConnectionHubGame.invoke("SendTurnSkipped", Guid).catch(function (err) { return console.error(err.toString()); });
};

function SendChrominoDrawn() {
    ConnectionHubGame.invoke("SendChrominoDrawn", Guid).catch(function (err) { return console.error(err.toString()); });
};

function SendBotChrominoPlayed(chrominoPlayed, isDrawn) {
    ConnectionHubGame.invoke("SendBotChrominoPlayed", Guid, chrominoPlayed, isDrawn).catch(function (err) { return console.error(err.toString()); });
};

function SendBotTurnSkipped(isDrawn) {
    ConnectionHubGame.invoke("SendBotTurnSkipped", Guid, isDrawn).catch(function (err) { return console.error(err.toString()); });
};