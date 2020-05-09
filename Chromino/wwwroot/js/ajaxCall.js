function TipClosePopup(popup, checkBox) {
    if ($(checkBox).is(":checked")) {
        let dontShowAllTips = false;
        if ($('input[name=dontShowAllTips]').is(':checked'))
            dontShowAllTips = true;
        $.ajax({
            url: '/Tip/Off',
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
        url: '/Memo/Add',
        type: 'POST',
        data: { gameId: GameId, memo: memoContent },
        success: function (data) { CallbackAddMemo(data) },
    });
}

function ChatAddMessage() {
    let message = $('#ChatInput').val();
    if (message != "") {
        $.ajax({
            url: '/Chat/PostMessage',
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
        success: function (data) { CallbackChatGetMessages(data, newMessages); }
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

function Help() {
    if ($(".Possible").length == 0) {
        $.ajax({
            url: '/Game/Help',
            type: 'POST',
            data: { gameId: GameId },
            success: function (data) { CallbackHelp(data); }
        });
    }
}

function SkipTurn() {
    HideButtonPlayChromino();
    $.ajax({
        url: '/Game/SkipTurn',
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackSkipTurn(data); },
    });
}

function DrawChromino() {
    $.ajax({
        url: '/Game/DrawChromino',
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackDrawChromino(data); },
    });
}

function End() {
    $.ajax({
        url: '/Game/End',
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackEnd(data); },
    });
}

function GetGameInfos(){
    $.ajax({
        url: '/Game/Infos',
        type: 'POST',
        async: false,
        data: { gameId: GameId },
        success: function (data) { CallbackGameInfos(data); },
    });
}

function GetInfosAfterPlaying() {
    $.ajax({
        url: '/Game/InfosAfterPlaying',
        type: 'POST',
        async: false,
        data: { gameId: GameId, playerId: PlayerTurn.id },
        success: function (data) { TEST(data); },
    });
}

//TODO DETTE TECHNIQUE
var TESTnextPlayerId;
var TESTlastChrominoColors;
var TESTfinish;
function TEST(data) {
    TESTnextPlayerId = data.nextPlayerId;
    TESTlastChrominoColors = data.lastChrominoColors;
    TESTfinish = data.finish;
}
// !TODO DETTE TECHNIQUE


function PlayingBot(botId) {
    if (!IsGameFinish) {
        let infoBotPlaying = Players.find(p => p.id == botId).name + " joue";
        $('#InfoGame').html(infoBotPlaying).fadeIn();
        ShowWorkInProgress();
        $.ajax({
            url: '/Game/PlayBot',
            type: 'POST',
            data: { id: GameId, botId: botId },
            success: function (data) { CallbackBotPlayed(data, botId); },
        });
    }
}

function PlayingChromino() {
    ShowInfoPopup = false;
    HideButtonPlayChromino();
    if (!IsGameFinish && LastChrominoMove != null) {
        IsPlayingBackEnd = true;
        StopDraggable(LastChrominoMove);
        ShowWorkInProgress();
        let offset = $(LastChrominoMove).offset();
        let xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        let yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
        let chrominoId = LastChrominoMove.id;
        let orientation;
        let flip;
        switch (GetAngle(LastChrominoMove)) {
            case 0:
                orientation = Orientation.horizontal;
                flip = false;
                break;
            case 90:
                orientation = Orientation.vertical;
                flip = false;
                yIndex--;
                break;
            case 180:
                orientation = Orientation.horizontal;
                flip = true;
                break;
            case 270:
                orientation = Orientation.vertical;
                flip = true;
                yIndex--;
                break;
            default:
                break;
        }
        let x = xIndex + XMin;
        let y = yIndex + YMin;
        $.ajax({
            url: '/Game/Play',
            type: 'POST',
            data: { gameId: GameId, chrominoId: chrominoId, x: x, y: y, orientation: orientation, flip: flip },
            success: function (data) { CallbackPlayChromino(data, chrominoId, xIndex, yIndex, orientation, flip); },
        });
    }
}
