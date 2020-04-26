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

function AddChat() {
    let chatAdd = $('#ChatInput').val();
    if (chatAdd != "") {
        $.ajax({
            url: UrlChatAdd,
            type: 'POST',
            data: { gameId: GameId, chat: chatAdd },
            success: function (data) { CallbackAddChat(data) },
        });
    }
}

function ReadChat() {
    if (NotReadMessages > 0) {
        $.ajax({
            url: UrlChatRead,
            type: 'POST',
            data: { gameId: GameId },
            success: function () { CallbackReadChat(); }
        });
    }
}

function Help() {
    $.ajax({
        url: UrlHelp,
        type: 'POST',
        data: { gameId: GameId },
        success: function (data) { CallbackHelp(data); }
    });
}

function SkipTurn() {
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

function PlayBot(botId) {
    if (!IsGameFinish) {
        $.ajax({
            url: UrlPlayBot,
            type: 'POST',
            data: { id: GameId, botId: botId },
            success: function (data) { CallbackPlayBot(data); },
        });
    }
}

function PlayChromino() {
    if (!IsGameFinish) {
        if (LastChrominoMove != null) {
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
}