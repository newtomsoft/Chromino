function ReceivePlayersInGame(newPlayersId) {
    Players.forEach(player => player.connected = false);
    newPlayersId.forEach(id => Players[Players.findIndex(p => p.id == id)].connected = true);
}

function OpponentMessageSent(guid) {
    if (guid != Guid)
        return;
    if ($("#PopupChat").is(":visible"))
        ChatGetMessages(true, true);
    else
        ChatGetMessages(true);
}

function OpponentChrominoPlayed(guid, chrominoPlayed) {
    if (guid != Guid)
        return;
    GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    let data = { finish: TESTfinish }
    UpdateInHandNumber(PlayerTurn.id, -1, TESTlastChrominoColors);
    RefreshVar(data);
    RefreshDom(true);
}

function OpponentChrominoDrawn(guid) {
    if (guid != Guid)
        return;
    GetInfosAfterPlaying();
    $('#InfoGame').html(PlayerTurn.name + " pioche...").fadeIn().delay(1000).fadeOut();
    DecreaseInStack();
    UpdateInHandNumber(PlayerTurn.id, 1);
    RefreshDom();
}

function OpponentTurnSkipped(guid) {
    if (guid != Guid)
        return;
    GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    let data = { finish: TESTfinish }
    RefreshVar(data);
    RefreshDom(true);
}

function BotChrominoPlayed(guid, chrominoPlayed, isDrawn) {
    if (guid != Guid)
        return;
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

function BotTurnSkipped(guid, isDrawn) {
    if (guid != Guid)
        return;
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