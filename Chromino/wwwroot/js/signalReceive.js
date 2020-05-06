function OpponentMessageSent(gameId) {
    if (gameId != GameId)
        return;
    if ($("#PopupChat").is(":visible"))
        ChatReadMessages();
    else
        ChatGetMessages(true);
}

function OpponentChrominoPlayed(gameId, chrominoPlayed) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurnName + " a posé");
    let data = { isBot: TESTisBot, finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    UpdateInHandNumber(PlayerTurnId, -1, TESTlastChrominoColors);
    RefreshVar(data);
    RefreshDom(true);
}

function OpponentChrominoDrawn(gameId) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    $('#InfoGame').html(PlayerTurnName + " pioche...").fadeIn().delay(1000).fadeOut();
    DecreaseInStack();
    UpdateInHandNumber(PlayerTurnId, 1);
    RefreshDom();
}

function OpponentTurnSkipped(gameId) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurnName + " a passé");
    let data = { isBot: TESTisBot, finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    RefreshVar(data);
    RefreshDom(true);
}