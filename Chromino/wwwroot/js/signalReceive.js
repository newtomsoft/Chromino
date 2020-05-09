function OpponentMessageSent() {
    if ($("#PopupChat").is(":visible"))
        ChatGetMessages(true, true);
    else
        ChatGetMessages(true);
}

function OpponentChrominoPlayed(chrominoPlayed) {
    GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    let data = { finish: TESTfinish }
    UpdateInHandNumber(PlayerTurn.id, -1, TESTlastChrominoColors);
    RefreshVar(data);
    RefreshDom(true);
}

function OpponentChrominoDrawn() {
    GetInfosAfterPlaying();
    $('#InfoGame').html(PlayerTurn.name + " pioche...").fadeIn().delay(1000).fadeOut();
    DecreaseInStack();
    UpdateInHandNumber(PlayerTurn.id, 1);
    RefreshDom();
}

function OpponentTurnSkipped() {
    GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    let data = { finish: TESTfinish }
    RefreshVar(data);
    RefreshDom(true);
}

function BotChrominoPlayed(chrominoPlayed, isDrawn) {
    GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    let data = { finish: TESTfinish }
    if (isDrawn)
        DecreaseInStack();
    else
        UpdateInHandNumber(PlayerTurn.id, -1, TESTlastChrominoColors);

    RefreshVar(data);
    RefreshDom(true);
}

function BotTurnSkipped(isDrawn) {
    GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    if (isDrawn) {
        DecreaseInStack();
        UpdateInHandNumber(PlayerTurn.id, 1);
    }
    let data = { finish: TESTfinish }
    RefreshVar(data);
    RefreshDom(true);
}