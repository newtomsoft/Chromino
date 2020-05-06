function OpponentMessageSent(gameId) {
    if (gameId != GameId)
        return;
    if ($("#PopupChat").is(":visible"))
        ChatReadMessages();
    else
        ChatGetMessages(true);
}

function OpponentChrominoPlayed(gameId, playerName, chrominoPlayed) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, playerName + " a posé");
    let data = { isBot: TESTisBot, finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    RefreshVar(data);
    RefreshDom(true);
}

function OpponentChrominoDrawn(gameId, playerName) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    $('#InfoGame').html(playerName + " pioche...").fadeIn().delay(1000).fadeOut();
    RefreshDom();
}

function OpponentTurnSkipped(gameId, playerName) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    AddHistorySkipTurn(playerName + " a passé");
    let data = { isBot: TESTisBot, finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    RefreshVar(data);
    RefreshDom(true);
}