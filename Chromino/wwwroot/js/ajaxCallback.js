function RefreshVar(data) {
    IsBot = data.isBot;
    PlayerTurnId = data.nextPlayerId;
}

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
    if (data.status) {
        HelpNumber--;
        HelpIndexes = data.indexes;
        RefreshDom();
    }
}


function CallbackDrawChromino(data) {
    if (data.id === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        AddChrominoInHand(data);
        if (PlayersNumber > 1) {
            HideButtonDrawChromino();
            ShowButtonSkipTurn();
        }
    }
}

function CallbackSkipTurn(data) {
    if (data.id === undefined) {
        ErrorReturn(player.errorReturn);
    }
    else {
        HideButtonSkipTurn();
        ShowButtonNextGame();
        IsBot = data.isBot;
        PlayerTurnId = data.id;
        RefreshDom();
    }
}

function CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip) {
    if (data.nextPlayerId === undefined) {
        ErrorReturn(data.errorReturn);
    }
    else {
        RefreshVar(data);
        if (PlayersNumber > 1) {
            HideButtonDrawChromino();
            HideButtonSkipTurn();
            ShowButtonNextGame();
        }
        HideButtonPlayChromino();
        if (true)
            AddChrominoInGame(xIndex, yIndex, orientation, flip, data.colors);
        else
            RefreshGameArea(data.squaresVM);
        RemoveChrominoInHand(chrominoId);
        RefreshDom();
    }
}

function CallbackPlayBot(data) {
    RefreshVar(data);
    if (!data.botSkip) {
        if (true) {
            let xIndex = data.x - XMin;
            let yIndex = data.y - YMin;
            AddChrominoInGame(xIndex, yIndex, data.orientation, data.flip, data.colors);
        }
        else
            RefreshGameArea(data.squaresVM);
    }
    if (PlayerTurnId == PlayerId) {
        ShowButtonDrawChromino();
        HideButtonSkipTurn();
        HideButtonNextGame();
    }
    RefreshDom();
}