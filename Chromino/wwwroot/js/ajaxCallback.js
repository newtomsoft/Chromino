function CallbackReadChat() {
    $('#NotifChat').text("0");
    $('#NotifChat').hide();
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

function CallbackAddChat(data) {
    $('#ChatPopupContent').val(data.chat);
    $('#ChatInput').val("");
}

function CallbackHelp(data) {
    if (data.indexes.length > 0) {
        HelpIndexes = data.indexes;
        HelpNumber--;
    }
    else {
        $('#PlayerHistoryPseudo').html("Pas d'emplacements possibles").fadeIn().delay(1000).fadeOut();
    }
    RefreshHelp();
}

function CallbackDrawChromino(data) {
    if (data.id === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        AddChrominoInHand(data);
        DecreaseInStack();
        UpdateInHandNumber(PlayerId, 1);
        if (PlayersNumber > 1) {
            HideButtonDrawChromino();
            ShowButtonSkipTurn();
        }
        else if (InStack == 0)
            HideButtonDrawChromino();
    }
}

function CallbackSkipTurn(data) {
    if (data.id === undefined) {
        ErrorReturn(player.errorReturn);
    }
    else {
        HideButtonSkipTurn();
        ShowButtonNextGame();
        AddHistorySkipTurn("Vous");
        IsBot = data.isBot;
        PlayerTurnId = data.id;
        RefreshVar(data);
        RefreshDom();
    }
}

function CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip) {
    if (data.nextPlayerId === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        if (PlayersNumber > 1) {
            HideButtonDrawChromino();
            HideButtonSkipTurn();
            ShowButtonNextGame();
        }
        HideButtonPlayChromino();
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: orientation, flip: flip, colors: data.colors }, "Vous");
        RemoveChrominoInHand(chrominoId);
        UpdateInHandNumber(PlayerId, -1, data.lastChrominoColors);
        RefreshVar(data);
        RefreshDom();
        WaitOpponentPlayed();
    }
}

function CallbackPlayBot(data) {
    if (data.botDraw) {
        DecreaseInStack();
        UpdateInHandNumber(PlayerTurnId, 1);
    }
    if (data.botSkip) {
        AddHistorySkipTurn(data.botName); // todo optimiser avec Players[x].name
    }
    else {
        let xIndex = data.x - XMin;
        let yIndex = data.y - YMin;
        AddChrominoInGame({ xIndex: xIndex, yIndex: yIndex, orientation: data.orientation, flip: data.flip, colors: data.colors }, data.botName);
        UpdateInHandNumber(PlayerTurnId, -1, data.lastChrominoColors);
    }
    RefreshVar(data);
    if (PlayerTurnId == PlayerId) {
        ShowButtonDrawChromino();
        HideButtonSkipTurn();
        HideButtonNextGame();
    }
    RefreshDom(true);
}

function CallbackEnd(data) {
    if (data.askRematch) {
        $("#AskRematch").show();
    }
}

function CallbackOpponentPlayed(data){
    alert("adversaire a joué id :" + data.id);
}

function DecreaseInStack() {
    InStack--;
    RefreshInStack();
}

function UpdateInHandNumber(playerId, step, lastChrominoColors) {
    let player = Players.find(p => p.id == playerId);
    player.chrominosNumber += step;
    if (player.chrominosNumber == 1) {
        player.lastChrominoColors = lastChrominoColors;
        if (player.name != "Vous")
            ShowInfoPopup = true;
    }
    UpdateInHandNumberDom(player);
}

function RefreshVar(data) {
    if (data !== undefined) {
        if (data.isBot !== undefined) IsBot = data.isBot;
        if (data.finish !== undefined) IsGameFinish = data.finish;
        if (data.nextPlayerId !== undefined) PlayerTurnId = data.nextPlayerId;
    }
    PlayerTurnName = Players.find(p => p.id == PlayerTurnId).name;
    PlayerTurnText = PlayerTurnName == "Vous" ? "C'est à vous de jouer" : `C'est à ${PlayerTurnName} de jouer`;
    if (IsGameFinish) {
        ShowInfoPopup = true;
        End();
    }
}