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
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    let data = { finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    UpdateInHandNumber(PlayerTurn.id, -1, TESTlastChrominoColors);
    RefreshVar(data);
    RefreshDom(true);
}

function OpponentChrominoDrawn(gameId) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    $('#InfoGame').html(PlayerTurn.name + " pioche...").fadeIn().delay(1000).fadeOut();
    DecreaseInStack();
    UpdateInHandNumber(PlayerTurn.id, 1);
    RefreshDom();
}

function OpponentTurnSkipped(gameId) {
    if (gameId != GameId)
        return;
    // TODO : pioche ??
    GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    let data = { finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    RefreshVar(data);
    RefreshDom(true);
}

function BotChrominoPlayed(gameId, chrominoPlayed, isDrawn) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    let data = { finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    if (isDrawn)
        DecreaseInStack();
    else
        UpdateInHandNumber(PlayerTurn.id, -1, TESTlastChrominoColors);

    RefreshVar(data);
    RefreshDom(true);
}

function BotTurnSkipped(gameId, isDrawn) {
    if (gameId != GameId)
        return;
    GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    if (isDrawn) {
        DecreaseInStack();
        UpdateInHandNumber(PlayerTurn.id, 1);
    }
    let data = { finish: TESTfinish, nextPlayerId: TESTnextPlayerId }
    RefreshVar(data);
    RefreshDom(true);
}