function SendAddToGame() {
    let guid = Guid !== undefined ? Guid : "NoGame";  
    ConnectionHubGame.invoke("SendAddToGame", guid).catch(function (err) { return console.error(err.toString()); });
};

function SendChatMessageSent() {
    ConnectionHubGame.invoke("SendChatMessageSent", Guid, HumansId).catch(function (err) { return console.error(err.toString()); });
};

function SendPrivateMessageSent(recipientId) {
    ConnectionHubGame.invoke("SendPrivateMessageSent", recipientId).catch(function (err) { return console.error(err.toString()); });
};

function SendChrominoPlayed(chrominoPlayed) {
    ConnectionHubGame.invoke("SendChrominoPlayed", Guid, HumansOpponentsId, chrominoPlayed).catch(function (err) { return console.error(err.toString()); });
};

function SendTurnSkipped() {
    ConnectionHubGame.invoke("SendTurnSkipped", Guid, HumansOpponentsId).catch(function (err) { return console.error(err.toString()); });
};

function SendChrominoDrawn() {
    ConnectionHubGame.invoke("SendChrominoDrawn", Guid, HumansOpponentsId).catch(function (err) { return console.error(err.toString()); });
};

function SendBotChrominoPlayed(chrominoPlayed, isDrawn) {
    ConnectionHubGame.invoke("SendBotChrominoPlayed", Guid, HumansOpponentsId, chrominoPlayed, isDrawn).catch(function (err) { return console.error(err.toString()); });
};

function SendBotTurnSkipped(isDrawn) {
    ConnectionHubGame.invoke("SendBotTurnSkipped", Guid, HumansOpponentsId, isDrawn).catch(function (err) { return console.error(err.toString()); });
};