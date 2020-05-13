function ReceivePlayersInGame(playersId) {
    Players.forEach(player => player.ongame = false);
    playersId.forEach(id => Players[Players.findIndex(p => p.id == id)].ongame = true);
    RefreshColorsPlayers();
    RefreshPlayersStatus();
    RefreshPopupPrivateMessage();
}

function ReceivePlayersLogged(playersId) {
    Players.forEach(player => player.online = false);
    playersId.forEach(id => Players[Players.findIndex(p => p.id == id)].online = true);
    RefreshPlayersLogged();
    RefreshPlayersStatus();
    RefreshPopupPrivateMessage();
}

function ReceiveChatMessageSent(guid) {
    if (guid != Guid)
        return;
    if ($("#PopupChat").is(":visible"))
        ChatGetMessages(true, true);
    else
        ChatGetMessages(true);
}

function ReceivePrivateMessageMessageSent(senderId) {
    if ( $(`#PopupPrivateMessage[penpalid='${ senderId }']`).is(":visible"))
        PrivateMessageGetMessages(true, true, senderId, false);
    else
        PrivateMessageGetMessages(true, false, senderId, false);
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