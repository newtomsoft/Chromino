function CallbackChatGetMessages(data) {
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

function CallbackAddChat(data) {
    $('#ChatPopupContent').val($('#ChatPopupContent').val() + data.newMessage);
    $('#ChatInput').val("");
    NotifyChat();
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
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: orientation, flip: flip, colors: data.colors }, "Vous avez posé");
        RemoveChrominoInHand(chrominoId);
        UpdateInHandNumber(PlayerId, -1, data.lastChrominoColors);
        RefreshVar(data);
        RefreshDom();
    }
}

function OpponentHavePlayed(data, playerId, isBot) {
    $('#InfoGame').fadeOut();
    let infoPlayerPlay = Players.find(p => p.id == playerId).name;
    if (data.draw) {
        DecreaseInStack();
        UpdateInHandNumber(playerId, 1);
        infoPlayerPlay += " a pioché et"
    }
    if (data.skip) {
        AddHistorySkipTurn(infoPlayerPlay + " a passé");
    }
    else {
        let xIndex = data.x - XMin;
        let yIndex = data.y - YMin;
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: data.orientation, flip: data.flip, colors: data.colors }, infoPlayerPlay + " a posé");
        UpdateInHandNumber(playerId, -1, data.lastChrominoColors);
    }
    if (isBot)
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