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

async function OpponentChrominoPlayed(guid, chrominoPlayed) {
    if (guid != Guid)
        return;
    let data = await GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    UpdateInHandNumber(PlayerTurn.id, -1, data.lastChrominoColors);
    RefreshVar({ finish: data.finish });
    RefreshDom(true);
}

async function OpponentChrominoDrawn(guid) {
    if (guid != Guid)
        return;
    await GetInfosAfterPlaying();
    $('#InfoGame').html(PlayerTurn.name + " pioche...").fadeIn().delay(1000).fadeOut();
    DecreaseInStack();
    UpdateInHandNumber(PlayerTurn.id, 1);
    RefreshDom();
}

async function OpponentTurnSkipped(guid) {
    if (guid != Guid)
        return;
    let data = await GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    RefreshVar({ finish: data.finish });
    RefreshDom(true);
}

async function BotChrominoPlayed(guid, chrominoPlayed, isDrawn) {
    if (guid != Guid)
        return;
    let data = await GetInfosAfterPlaying();
    AddChrominoInGame(chrominoPlayed, PlayerTurn.name + " a posé");
    if (isDrawn)
        DecreaseInStack();
    else
        UpdateInHandNumber(PlayerTurn.id, -1, data.lastChrominoColors);
    RefreshVar({ finish: data.finish });
    RefreshDom(true);
}

async function BotTurnSkipped(guid, isDrawn) {
    if (guid != Guid)
        return;
    let data = await GetInfosAfterPlaying();
    AddHistorySkipTurn(PlayerTurn.name + " a passé");
    if (isDrawn) {
        DecreaseInStack();
        UpdateInHandNumber(PlayerTurn.id, 1);
    }
    RefreshVar({ finish: data.finish });
    RefreshDom(true);
}