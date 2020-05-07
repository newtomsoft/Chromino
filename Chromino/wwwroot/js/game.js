var TimeoutPut = null;
var ToPut = false;
var PositionLastChromino;
var TimeoutRotate = null;
var ToRotate = true;
var LastChrominoMove = null;
var OffsetLastChromino = 0;
var OffsetGameArea = null;
var GameAreaOffsetX;
var GameAreaOffsetY;
var SquareSize;
var Tip;
var TimeoutValidateChromino = null;
var IndexMove = 0;
var HelpIndexes = new Array;
var Players;
var IsPlayingBackEnd;
var OpponentsAllBots;

$(document).ready(function () {
    InitDom();
    CallSignalR();
    RefreshDom();
});

function InitDom() {
    GetPlayersInfos();
    ShowButtonChat();
    RefreshButtonNextGame();
    $("#MemoAdd").click(AddMemo);
    $("#ChatAdd").click(ChatAddMessage);
    $(".doAction").click(function () { Action(this.id); });
    $("#TipClose").click(function () { TipClosePopup('#TipPopup', '#TipDontShowAgain'); });
    HistoryChrominos.UpdateSquares = function (addNumber, columnsNumber, placeToAdd) {
        this.forEach(function (item) {
            if (item.square0 !== undefined) {
                for (i = 0; i < 3; i++) {
                    squareProp = "square" + i;
                    oldNumber = parseInt(item[squareProp].replace("Square_", ""));
                    switch (placeToAdd) {
                        case 'top':
                            item[squareProp] = "Square_" + (oldNumber + addNumber * columnsNumber);
                            break;
                        case 'right':
                            item[squareProp] = "Square_" + (oldNumber + addNumber * Math.floor(oldNumber / columnsNumber));
                            break;
                        case 'left':
                            item[squareProp] = "Square_" + (oldNumber + addNumber * (1 + Math.floor(oldNumber / columnsNumber)));
                            break;
                    }
                }
            }
        });
    };
    Action("Welcome");
    RefreshVar();
    Players.forEach(function (player) {
        UpdateInHandNumberDom(player);
    });
    RefreshInfoPopup();
    $("#ButtonPreviousChromino").click(function () {
        if (Tips.find(x => x.elementId == "HistoryChrominos") == undefined) {
            if (IndexMove < HistoryChrominos.length - 1) {
                IndexMove++;
            }
            AnimateChromino(2, true);
            HideChrominosPlayed();
        }
    });
    $("#ButtonNextChromino").click(function () {
        if (Tips.find(x => x.elementId == "HistoryChrominos") == undefined) {
            if (IndexMove > 0) {
                IndexMove--;
            }
            AnimateChromino(2, true);
        }
    });
    $(document).click(function () {
        if (!IsPlayingBackEnd) {
            StopDraggable();
            StartDraggable();
        }
    });
    $(document).mouseup(function () {
        MagnetChromino();
    });
    $(window).resize(function () {
        ResizeGameArea();
    });
    window.oncontextmenu = function (event) {     // désactivation du menu contextuel clic droit
        event.preventDefault();
        event.stopPropagation();
    };
    $('#ChatInput').on('keydown', function (e) {
        if (e.which == 13) {     // touche entrer sur chat
            ChatAddMessage();
        }
    });
    ChatGetMessages(false);
    $(".emoji").click(function () {
        let selectionStart = $('#ChatInput').prop("selectionStart");
        let text = $('#ChatInput').val();
        let textBefore = text.substring(0, selectionStart);
        let textAfter = text.substring(selectionStart, text.length);
        $('#ChatInput').val(textBefore + $(this).html() + textAfter);
        $('#ChatInput').focus();
    });
}

//***************************************************//
//**** gestion affichage derniers chrominos joués ***//
//***************************************************//

function AnimateChromino(loopNumber, history, historyReset) {
    if (historyReset)
        IndexMove = 0;
    let index = history ? IndexMove : 0;
    if (HistoryChrominos.length != 0) {
        ShowSquare('#' + HistoryChrominos[index].square0);
        ShowSquare('#' + HistoryChrominos[index].square1);
        ShowSquare('#' + HistoryChrominos[index].square2);
        for (let i = 0; i < loopNumber; i++) {
            AnimateSquare('#' + HistoryChrominos[index].square0);
            AnimateSquare('#' + HistoryChrominos[index].square1);
            AnimateSquare('#' + HistoryChrominos[index].square2);
        }
        if (history) {
            $('#InfoGame').html("");
            $('#PlayerHistoryPseudo').css('opacity', '1');
            $('#PlayerHistoryPseudo').dequeue();
            $('#PlayerHistoryPseudo').html(HistoryChrominos[index].infoPlayerPlay).fadeIn().delay(1000).fadeOut();
        }
    }
}

function ShowLastChrominoPlayed() {
    AnimateChromino(0);
}

function AnimateSquare(squareId) {
    $(squareId).fadeToggle(170, function () {
        $(this).fadeToggle(170);
    });
}

function HideChrominosPlayed() {
    if (IndexMove > 0) {
        HideSquare('#' + HistoryChrominos[IndexMove - 1].square0);
        HideSquare('#' + HistoryChrominos[IndexMove - 1].square1);
        HideSquare('#' + HistoryChrominos[IndexMove - 1].square2);
    }
}

function HideSquare(squareId) {
    let parentId = '#' + $(squareId).parent().attr("id");
    let parentTopId = '#' + $(parentId).prev().attr("id");
    let parentBottomId = '#' + $(parentId).next().attr("id");
    let indexSquare = $(squareId).index() + 1;
    let nthChild = " :nth-child(" + indexSquare + ")";
    let squareTop = $(parentTopId + nthChild);
    let squareBottom = $(parentBottomId + nthChild);
    let squareLeft = $(squareId).prev();
    let squareRight = $(squareId).next();

    $(squareId).addClass("OpenAll CloseNone Free");
    if ($(squareLeft).hasClass("Free"))
        $(squareLeft).removeClass("CloseRight");
    else
        $(squareId).addClass("CloseLeft");
    if ($(squareRight).hasClass("Free"))
        $(squareRight).removeClass("CloseLeft");
    else
        $(squareId).addClass("CloseRight");
    if ($(squareTop).hasClass("Free"))
        $(squareTop).removeClass("CloseBottom");
    else
        $(squareId).addClass("CloseTop");
    if ($(squareBottom).hasClass("Free"))
        $(squareBottom).removeClass("CloseTop");
    else
        $(squareId).addClass("CloseBottom");
}

function ShowSquare(squareId) {
    let parentId = '#' + $(squareId).parent().attr("id");
    let parentTopId = '#' + $(parentId).prev().attr("id");
    let parentBottomId = '#' + $(parentId).next().attr("id");
    let indexSquare = $(squareId).index() + 1;
    let nthChild = " :nth-child(" + indexSquare + ")";
    let squareTop = $(parentTopId + nthChild);
    let squareBottom = $(parentBottomId + nthChild);
    let squareLeft = $(squareId).prev();
    let squareRight = $(squareId).next();
    if ($(squareLeft).hasClass("Free"))
        $(squareLeft).addClass("CloseRight");
    else
        $(squareId).removeClass("CloseLeft");
    if ($(squareRight).hasClass("Free"))
        $(squareRight).addClass("CloseLeft");
    else
        $(squareId).removeClass("CloseRight");
    if ($(squareTop).hasClass("Free"))
        $(squareTop).addClass("CloseBottom");
    else
        $(squareId).removeClass("CloseTop");
    if ($(squareBottom).hasClass("Free"))
        $(squareBottom).addClass("CloseTop");
    else
        $(squareId).removeClass("CloseBottom");
    $(squareId).removeClass("OpenAll CloseNone Free");
}

//***************************************************//
//** gestion déplacements / rotation des chrominos **//
//***************************************************//
function StartDraggable() {
    if (Tips.find(x => x.elementId == "Hand") == undefined) {
        $(".handPlayerChromino").draggableTouch()
            .on("dragstart", function () {
                $(this).css('cursor', 'grabbing');
                ScheduleRotate();
                LastChrominoMove = this;
                PositionLastChromino = $(LastChrominoMove).offset();
                SchedulePut();
                StopScheduleValidateChromino();
            }).on("dragend", function () {
                if (IsChrominoInGameArea(this) && PlayerId == PlayerTurn.id && !IsPlayingBackEnd && !IsGameFinish) {
                    ShowButtonPlayChromino();
                    ScheduleValidateChromino();
                }
                else {
                    HideButtonPlayChromino();
                }
                if ($(this).css('cursor') == 'grabbing')
                    $(this).css('cursor', 'grab');
                LastChrominoMove = this;
                OffsetLastChromino = $(this).offset();
                MagnetChromino();
                if (ToRotate) {
                    ToRotate = false;
                    clearTimeout(TimeoutRotate);
                    Rotation(LastChrominoMove);
                }
                let offset = $(LastChrominoMove).offset();
                if (ToPut && PositionLastChromino.left == offset.left && PositionLastChromino.top == offset.top) {
                    clearTimeout(TimeoutPut);
                    PlayingChromino();
                }
                ToPut = false;
            });
    }
}

function StopDraggable(chromino) {
    if (Tips.find(x => x.elementId == "Hand") == undefined) {
        $(this).off("mouseup");
        $(this).off("mousedown");
        $(document).off("mousemove");
        if (chromino === undefined)
            $('.handPlayerChromino').draggableTouch("disable");
        else
            $(chromino).draggableTouch("disable");
    }
}

function ScheduleRotate() {
    ToRotate = true;
    clearTimeout(TimeoutRotate);
    TimeoutRotate = setTimeout(function () {
        ToRotate = false;
    }, 120);
}

function SchedulePut() {
    if (PlayerTurn.id == PlayerId && !IsPlayingBackEnd) {
        clearTimeout(TimeoutPut);
        PositionLastChromino = $(LastChrominoMove).offset();
        TimeoutPut = setTimeout(function () {
            let position = $(LastChrominoMove).offset();
            if (PositionLastChromino.left == position.left && PositionLastChromino.top == position.top) {
                ToPut = true;
                ShowOkToPut();
            }
        }, 600);
    }
}

function ShowOkToPut() {
    $('#GameArea').fadeToggle(25, function () {
        $(this).fadeToggle(25);
    });
    $(LastChrominoMove).fadeToggle(25, function () {
        $(this).fadeToggle(25);
    });
}

function MagnetChromino() {
    if (LastChrominoMove != null) {
        let offset = $(LastChrominoMove).offset();
        let x = offset.left - GameAreaOffsetX;
        let y = offset.top - GameAreaOffsetY;
        let difX = SquareSize * Math.round(x / SquareSize) - x;
        let difY = SquareSize * Math.round(y / SquareSize) - y;
        $(LastChrominoMove).css({ "left": "+=" + difX + "px", "top": "+=" + difY + "px" });
    }
}

function Rotation(chromino) {
    let angle = 90;
    let newAngle = angle + GetAngle(chromino);
    if (newAngle >= 360)
        newAngle -= 360;
    SetAngle(chromino, newAngle);
    if (GetOrientation(chromino) == "horizontal")
        SetOffset(chromino, -SquareSize, 0);
    else
        SetOffset(chromino, SquareSize, 0);
}

function SetOffset(chromino, left, top) {
    let offset = $(chromino).offset();
    let newLeft = offset.left + left;
    let newTop = offset.top + top;
    $(chromino).offset({ top: newTop, left: newLeft });
}

function GetOrientation(chromino) {
    switch (GetAngle(chromino)) {
        case 0:
        case 180:
            return "horizontal";
            break;
        case 90:
        case 270:
            return "vertical";
            break;
    }
}

function GetAngle(chromino) {
    switch ($(chromino).css("flex-direction")) {
        case "row":
            return 0
            break;
        case "column":
            return 90;
            break;
        case "row-reverse":
            return 180;
            break;
        case "column-reverse":
        default:
            return 270;
            break;
    }
}

function SetAngle(chromino, rotate) {
    let classChromino = '#' + $(chromino).attr('id');
    SetAngleByClass(classChromino, rotate);
}

function SetAngleByClass(classChromino, rotate) {
    switch (rotate) {
        case 0:
            $(classChromino).css("flex-direction", "row");
            $(classChromino + " div:nth-child(1)").removeClass("OpenTop").addClass("OpenRight");
            $(classChromino + " div:nth-child(2)").removeClass("OpenBottomTop").addClass("OpenRightLeft");
            $(classChromino + " div:nth-child(3)").removeClass("OpenBottom").addClass("OpenLeft");
            break;
        case 90:
            $(classChromino).css("flex-direction", "column");
            $(classChromino + " div:nth-child(1)").removeClass("OpenRight").addClass("OpenBottom");
            $(classChromino + " div:nth-child(2)").removeClass("OpenRightLeft").addClass("OpenBottomTop");
            $(classChromino + " div:nth-child(3)").removeClass("OpenLeft").addClass("OpenTop");
            break;
        case 180:
            $(classChromino).css("flex-direction", "row-reverse");
            $(classChromino + " div:nth-child(1)").removeClass("OpenBottom").addClass("OpenLeft");
            $(classChromino + " div:nth-child(2)").removeClass("OpenBottomTop").addClass("OpenRightLeft");
            $(classChromino + " div:nth-child(3)").removeClass("OpenTop").addClass("OpenRight");
            break;
        case 270:
            $(classChromino).css("flex-direction", "column-reverse");
            $(classChromino + " div:nth-child(1)").removeClass("OpenLeft").addClass("OpenTop");
            $(classChromino + " div:nth-child(2)").removeClass("OpenRightLeft").addClass("OpenBottomTop");
            $(classChromino + " div:nth-child(3)").removeClass("OpenRight").addClass("OpenBottom");
            break;
    }
}

//***************************************//
//********* fonctions GameArea  *********//
//***************************************//
function ResizeGameArea() {
    var documentWidth = $(document).width();
    var documentHeight = $(document).height();
    var width = documentWidth;
    var height = documentHeight;
    var offset = 0;
    if (width > height) {
        width -= 200; //-200 : somme de la taille des 2 bandeaux
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").css({ left: documentWidth - SquareSize * 4 }); //3 SquareSize pour le chromino + 1 de marge à droite
        $(".handPlayerChromino").each(function () {
            $(this).css({ top: offset });
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
        SetAngleByClass(".handPlayerChromino", 0);
    }
    else {
        height -= 200;
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").css({ top: documentHeight - SquareSize * 3 }); // marge d'1 SquareSize en bas implicite par la rotation (matrix)
        $(".handPlayerChromino").each(function () {
            $(this).css({ left: offset });
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
        SetAngleByClass(".handPlayerChromino", 90);
    }
    $('#GameArea').height(SquareSize * GameAreaLinesNumber);
    $('#GameArea').width(SquareSize * GameAreaColumnsNumber);
    $('.gameLineArea').outerHeight("auto");
    $('.Square').outerHeight(SquareSize);
    $('.Square').outerWidth(SquareSize);
    $('.handPlayerChromino').outerHeight(SquareSize);
    $('#GameArea').show();
    $('.gameLineArea').css('display', 'flex');
    var gameAreaOffset = $('#GameArea').offset();
    GameAreaOffsetX = gameAreaOffset.left;
    GameAreaOffsetY = gameAreaOffset.top;
    OffsetGameArea = $('#GameArea').offset();
}

function IsChrominoInGameArea(chromino) {
    let offsetRight = 1.5 * SquareSize;
    let offsetLeft, offsetTop, offsetBottom;
    if (GetOrientation(chromino) == "horizontal") {
        offsetLeft = 1.5 * SquareSize;
        offsetTop = -0.5 * SquareSize;
        offsetBottom = 1.5 * SquareSize;
    }
    else {
        offsetLeft = -0.5 * SquareSize;
        offsetTop = 0.5 * SquareSize;
        offsetBottom = 0.5 * SquareSize;
    }
    let heightGameArea = $('#GameArea').height();
    let widthGameArea = $('#GameArea').width();
    let offsetLastChromino = $(chromino).offset();
    if (offsetLastChromino.left + offsetLeft > OffsetGameArea.left && offsetLastChromino.left + offsetRight < OffsetGameArea.left + widthGameArea && offsetLastChromino.top + offsetTop > OffsetGameArea.top && offsetLastChromino.top + offsetBottom < OffsetGameArea.top + heightGameArea)
        return true;
    else
        return false;
}

//***************************************************//
//********** gestion popups de la partie ************//
//***************************************************//
function ShowPopup(popup, hasCloseButton = true) {
    StopScheduleValidateChromino();
    $(popup).show();
    $(popup).popup({ closebutton: hasCloseButton, autoopen: true, transition: 'all 0.4s' });
    $.fn.popup.defaults.pagecontainer = '#page';
    if (popup == '#PopupChat') {
        ChatReadMessages();
        $('#ChatPopupContent').scrollTop(300);
        let timeoutScroll;
        clearTimeout(timeoutScroll);
        timeoutScroll = setTimeout(function () {
            $('#ChatPopupContent').scrollTop($('#ChatPopupContent')[0].scrollHeight);
        }, 50);

    }
}

function ClosePopup(popup) {
    $(popup).popup('hide');
}

function ScheduleValidateChromino() {
    clearTimeout(TimeoutValidateChromino);
    TimeoutValidateChromino = setTimeout(function () {
        Action("ValidateChromino");
    }, 4000);

}

function StopScheduleValidateChromino() {
    if (TimeoutValidateChromino) {
        clearTimeout(TimeoutValidateChromino);
        TimeoutValidateChromino = 0;
    }
}

function ShowTipFeature(isCheck, hasCloseButton) {
    if (Tip.headPictureClass != "") {
        $('#TipHeadPicture').removeClass().addClass("div-head " + Tip.headPictureClass);
        $('#TipHeadPicture').show();
    }
    else {
        $('#TipHeadPicture').hide();
    }
    $('#TipHtml').html(Tip.description);
    $('#TipDontShowAgain').prop('checked', isCheck);
    if (Tip.illustrationPictureClass != "") {
        $('#TipIllustration').removeClass().addClass("illustration " + Tip.illustrationPictureClass);
        $('#TipIllustration').show();
    }
    else {
        $('#TipIllustration').hide();
    }
    ShowPopup('#TipPopup', hasCloseButton);
}

function Action(elementId) {
    Tip = Tips.find(x => x.elementId == elementId);
    let functionName = elementId.replace("Button", "");
    let form = "#Form" + functionName;
    let popup = "#Popup" + functionName;
    if (Tip != undefined)
        ShowTipFeature(true, false);
    else if (typeof window[functionName] === "function")
        window[functionName]();
    else if ($(popup).length)
        ShowPopup(popup);
    else if ($(form).length)
        $(form).submit();
}

function ErrorReturn(playReturn) {
    let index = PlayErrors.findIndex(x => x.name == playReturn)
    if (index >= 0) {
        $('#ErrorText').html(PlayErrors[index].description);
        $('#ErrorIllustration').removeClass().addClass("illustration " + PlayErrors[index].illustrationPictureClass);
        $('#ErrorIllustrationCaption').html(PlayErrors[index].illustrationPictureCaption);
        if (PlayErrors[index].illustrationPictureClass != "") {
            $('#ErrorIllustration').show();
            $('#ErrorIllustrationCaption').show();
        }
        else {
            $('#ErrorIllustration').hide();
            $('#ErrorIllustrationCaption').hide();
        }
        ShowPopup('#PopupError');
    }
    IsPlayingBackEnd = false;
}

function HaveBotResponsability() {
    let playerIndex = Players.findIndex(p => p.id == PlayerId);
    let opponentIndex = Players.findIndex(p => p.id == PlayerTurn.id);

    if (playerIndex < opponentIndex) {
        for (var i = playerIndex + 1; i <= opponentIndex; i++)
            if (!Players[i].isBot)
                return false;
    }
    else {
        for (var i = playerIndex + 1; i < Players.length; i++)
            if (!Players[i].isBot)
                return false;
        for (var i = 0; i <= opponentIndex; i++)
            if (!Players[i].isBot)
                return false;
    }
    return true;
}