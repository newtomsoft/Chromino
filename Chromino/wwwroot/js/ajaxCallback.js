function CallbackChatGetMessages(data, newMessages) {
    if (!newMessages)
        $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.chat);
    if (data.newMessagesNumber > 0) {
        $('#NotifChat').text(data.newMessagesNumber);
        $('#NotifChat').show();
    }
    else {
        $('#NotifChat').hide();
    }
}

function CallbackChatReadMessages(data) {
    $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.chat);
}

function CallbackAddMessage(data) {
    $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.newMessage);
    $('#ChatInput').val("");
    SendMessageSent();
}

function CallbackTipClosePopup(dontShowAllTips) {
    if (dontShowAllTips)
        Tips = new Array;
    else
        Tips.splice(Tips.findIndex(x => x.id == Tip.id), 1);
}

function CallbackAddMemo(data) {
    ClosePopup("#PopupMemo");
    if (data.memosNumber != 0) {
        $('#NotifMemo').text(data.memosNumber);
        $('#NotifMemo').show();
    }
    else {
        $('#NotifMemo').text(0);
        $('#NotifMemo').hide();
    }
}

function CallbackHelp(data) {
    if (data.indexes.length > 0) {
        HelpIndexes = data.indexes;
        HelpNumber--;
    }
    else {
        $('#InfoGame').html("Pas d'emplacements possibles").fadeIn().delay(1000).fadeOut();
    }
    RefreshHelp();
}

function CallbackDrawChromino(data) {
    if (data.id === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        HaveDraw = true;
        ShowInfoPopup = false;
        AddChrominoInHand(data);
        DecreaseInStack();
        UpdateInHandNumber(PlayerId, 1);
        StopDraggable();
        StartDraggable();
        if (!OpponentsAreBots && Players.length > 1)
            SendChrominoDrawn();
        RefreshDom();
    }
}

function CallbackSkipTurn(data) {
    if (data.id === undefined) {
        ErrorReturn(player.errorReturn);
    }
    else {
        RefreshButtonNextGame();
        AddHistorySkipTurn("Vous avez passé");
        IsBotTurn = data.isBot;
        PlayerTurnId = data.id;
        if (!OpponentsAreBots && Players.length > 1)
            SendTurnSkipped();
        RefreshVar(data);
        RefreshDom();
    }
}

function CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip) {
    IsPlayingBackEnd = false;
    ShowWorkIsFinish();
    if (data.nextPlayerId === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        HideButtonPlayChromino();
        let chrominoPlayed = { xIndex: xIndex, yIndex: yIndex, orientation: orientation, flip: flip, colors: data.colors };
        let chrominoPlayedCopy = { xIndex: xIndex, yIndex: yIndex, orientation: orientation, flip: flip, colors: data.colors };
        AddChrominoInGame(chrominoPlayed, "Vous avez posé");
        RemoveChrominoInHand(chrominoId);
        UpdateInHandNumber(PlayerId, -1, data.lastChrominoColors);
        if (!OpponentsAreBots && Players.length > 1)
            SendChrominoPlayed(chrominoPlayedCopy);
        RefreshVar(data);
        RefreshDom();
    }
}

function BotPlayed(data, botId) {
    $('#InfoGame').fadeOut();
    let infoBotPlay = Players.find(p => p.id == botId).name;
    if (data.draw) {
        DecreaseInStack();
        UpdateInHandNumber(botId, 1);
        infoBotPlay += " a pioché et"
    }
    if (data.skip) {
        AddHistorySkipTurn(infoBotPlay + " a passé");
    }
    else {
        let xIndex = data.x - XMin;
        let yIndex = data.y - YMin;
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: data.orientation, flip: data.flip, colors: data.colors }, infoBotPlay + " a posé");
        UpdateInHandNumber(botId, -1, data.lastChrominoColors);
    }
    ShowWorkIsFinish();
    RefreshVar(data);
    RefreshDom(true);
}

function CallbackEnd(data) {
    if (OpponentsAreBots) {
        $("#Askrematch-text").html("Rejouer ?");
        $("#Askrematch").show();
    }
    else if (data.askRematch) {
        $("#Askrematch-text").html("Prendre votre revanche ?");
        $("#Askrematch").show();
    }
}

function DecreaseInStack() {
    InStack--;
    RefreshInStack();
}

function UpdateInHandNumber(playerId, step, lastChrominoColors) {
    let player = Players.find(p => p.id == playerId);
    player.chrominosNumber += step;
    if (player.chrominosNumber <= 1) {
        player.lastChrominoColors = lastChrominoColors;
        if (player.name != "Vous")
            ShowInfoPopup = true;
    }
    UpdateInHandNumberDom(player);
}

function RefreshVar(data) {
    if (data !== undefined) {
        if (data.isBot !== undefined) IsBotTurn = data.isBot;
        if (data.finish !== undefined) IsGameFinish = data.finish;
        if (data.nextPlayerId !== undefined) PlayerTurnId = data.nextPlayerId;
    }
    PlayerTurnName = Players.find(p => p.id == PlayerTurnId).name;
    PlayerTurnText = PlayerTurnName == "Vous" ? "C'est à vous de jouer" : `C'est à ${PlayerTurnName} de jouer`;
    if (PlayerTurnId != PlayerId)
        HaveDraw = false;
    if (IsGameFinish) {
        ShowInfoPopup = true;
        End();
    }
}