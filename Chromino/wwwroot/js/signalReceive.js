function ReceivePlayersStatus(status, playersId) {
    Players.forEach(player => player[status] = false);
    playersId.forEach(id => Players[Players.findIndex(p => p.id == id)][status] = true);
    Contacts.forEach(h => h[status] = false);
    for (const id of playersId) {
        let index = Contacts.findIndex(h => h.id == id);
        if (index != -1)
            Contacts[index][status] = true;
    }
    RefreshColorsPlayers();
    RefreshPlayersStatusIndicator();
    RefreshPenpalTitleInPopupPrivateMessage();
}


function ReceiveChatMessageSent(guid) {
    if (guid != Guid)
        return;
    if ($('#ChatPopupContent').is(":visible"))
        GetChatMessages(true, true);
    else
        GetChatMessages(true);
}

function ReceivePrivateMessageMessageSent(senderId) {
    if ($('#PrivateMessagePopupContent').is(":visible") && $("#PrivateMessageAdd").attr('recipientId') == senderId)
        GetPrivateMessageMessages(true, true, senderId, false);
    else
        GetPrivateMessageMessages(true, false, senderId, false);
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