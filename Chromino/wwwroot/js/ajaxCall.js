function TipClosePopup(popup, checkBox) {
    if ($(checkBox).is(":checked")) {
        let dontShowAllTips = false;
        if ($('input[name=dontShowAllTips]').is(':checked'))
            dontShowAllTips = true;
        $.ajax({
            url: UrlTipOff,
            type: 'POST',
            data: { gameId: GameId, tipId: Tip.id, dontShowAllTips: dontShowAllTips },
            success: function () { CallbackTipClosePopup(dontShowAllTips) },
        });
    }
    ClosePopup(popup);
}

function AddMemo() {
    let memoContent = $('#MemoContent').val();
    $.ajax({
        url: UrlMemoAdd,
        type: 'POST',
        data: { gameId: GameId, memo: memoContent },
        success: function (data) { CallbackAddMemo(data) },
    });
}

function ChatAddMessage() {
    let message = $('#ChatInput').val();
    if (message != "") {
        $.ajax({
            url: UrlChatPostMessage,
            type: 'POST',
            data: { gameId: GameId, message: message },
            success: function (data) { CallbackAddMessage(data) },
        });
    }
}

function ChatGetMessages(newMessages) {
    $.ajax({
        url: UrlChatGetMessages,
        type: 'POST',
        data: { gameId: GameId, newMessages: newMessages },
        success: function (data) { CallbackChatGetMessages(data); }
    });
}

function ChatReadMessages() {
    $("#NotifChat").text("0");
    $("#NotifChat").hide();
    $.ajax({
        url: UrlChatGetMessages,
        type: 'POST',
        data: { gameId: GameId, newMessages: true, resetNotification: true },
        success: function (data) { CallbackChatReadMessages(data); }
    });
}


function SetLatestReadMessage() {
    //todo
    //date: new Date(Date.now()).toISOString() 
}


function Help() {
    if ($(".Possible").length == 0) {
        $.ajax({
            url: UrlHelp,
            type: 'POST',
            data: { gameId: GameId },
            success: function (data) { CallbackHelp(data); }
        });
    }
}

function SkipTurn() {
    HideButtonPlayChromino();
    $.ajax({
        url: UrlSkip,
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackSkipTurn(data); },
    });
}

function DrawChromino() {
    $.ajax({
        url: UrlDraw,
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackDrawChromino(data); },
    });
}

function End() {
    $.ajax({
        url: UrlEnd,
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackEnd(data); },
    });
}

function GetPlayersInfos() {
    $.ajax({
        url: UrlPlayersIdChrominosNumber,
        type: 'POST',
        async: false,
        data: { gameId: GameId },
        success: function (data) { Players = data; },
    });
}

function PlayingBot(botId) {
    if (!IsGameFinish) {
        let infoBotPlaying = Players.find(p => p.id == botId).name + " joue";
        $('#InfoGame').html(infoBotPlaying).fadeIn();
        ShowWorkInProgress();
        $.ajax({
            url: UrlPlayBot,
            type: 'POST',
            data: { id: GameId, botId: botId },
            success: function (data) { OpponentHavePlayed(data, botId, true); },
        });
    }
}

function PlayingOpponent() {
    if (PlayersNumber != 1 && !IsBotTurn) {
        $.ajax({
            url: UrlWaitOpponentPlayed,
            type: 'POST',
            data: { gameId: GameId, playerTurnId: PlayerTurnId },
            success: function (data) { OpponentHavePlayed(data, PlayerTurnId); },
        });
    }
}

function PlayingChromino() {
    ShowInfoPopup = false;
    HideButtonPlayChromino();
    if (!IsGameFinish && LastChrominoMove != null) {
        IsCanPlay = false;
        StopDraggable(LastChrominoMove);
        IsPlayingBackEnd = true;
        ShowWorkInProgress();
        let offset = $(LastChrominoMove).offset();
        let xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        let yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
        let chrominoId = LastChrominoMove.id;
        let orientation;
        let flip;
        switch (GetAngle(LastChrominoMove)) {
            case 0:
                orientation = Horizontal;
                flip = false;
                break;
            case 90:
                orientation = Vertical;
                flip = false;
                yIndex--;
                break;
            case 180:
                orientation = Horizontal;
                flip = true;
                break;
            case 270:
                orientation = Vertical;
                flip = true;
                yIndex--;
                break;
            default:
                break;
        }
        let x = xIndex + XMin;
        let y = yIndex + YMin;
        $.ajax({
            url: UrlPlay,
            type: 'POST',
            data: { gameId: GameId, chrominoId: chrominoId, x: x, y: y, orientation: orientation, flip: flip },
            success: function (data) { CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip); },
        });
    }
}
