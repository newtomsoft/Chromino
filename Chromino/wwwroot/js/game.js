$(document).ready(function () {
    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    $(document).mouseup(function () {
        MagnetChromino();
    });

    $(window).resize(function () {
        ResizeGameArea();
    });

    ResizeGameArea();
    StartDraggable();
    Action("Welcome");

    //animatation des derniers chromino joués
    if (!PreviouslyDraw && ThisPlayerTurn) {
        AnimateChrominosPlayed(0);
    }
    $("#ButtonPreviousChromino").click(function () {
        if (Tips.find(x => x.elementId == "HistoryChrominos") == undefined) {
            if (IndexMove < HistoryChrominos.length - 1) {
                IndexMove++;
            }
            AnimateChrominosPlayed();
            HideChrominosPlayed();
        }
    });
    $("#ButtonNextChromino").click(function () {
        if (Tips.find(x => x.elementId == "HistoryChrominos") == undefined) {
            if (IndexMove > 0) {
                IndexMove--;
            }
            AnimateChrominosPlayed();
        }
    });

    // désactivation du menu contextuel clic droit
    window.oncontextmenu = function (event) {
        event.preventDefault();
        event.stopPropagation();
    };

    // affichage notif nombre de messages non lus
    if (NotReadMessages != 0) {
        $('#NotifChat').text(NotReadMessages);
        $('#NotifChat').show();
    }

    // affichage notif nombres de mémos
    if (MemosNumber != 0) {
        $('#NotifMemo').text(MemosNumber);
        $('#NotifMemo').show();
    }

    // affichage popup
    if (PlayReturn != "Ok")
        ShowPopup('#errorPopup');
    else if (ShowInfoPopup)
        ShowPopup('#PopupInfo');
    else if (ShowBotPlayingInfoPopup)
        ShowPopup('#botPlayingInfoPopup');

    //emojis du chat
    $(".emoji").click(function () {
        let selectionStart = $("#Chat-input").prop("selectionStart");
        let text = $("#Chat-input").val();
        let textBefore = text.substring(0, selectionStart);
        let textAfter = text.substring(selectionStart, text.length);
        $('#Chat-input').val(textBefore + $(this).html() + textAfter);
        $("#Chat-input").focus();
    });


    //clic sur fermeture fenetre tip




});

//***************************************************//
//**** gestion affichage derniers chrominos joués ***//
//***************************************************//
let IndexMove = 0;
function AnimateChrominosPlayed() {
    for (let i = 0; i < 2; i++) {
        ShowSquare('#' + HistoryChrominos[IndexMove].square0);
        AnimateSquare('#' + HistoryChrominos[IndexMove].square0);
        ShowSquare('#' + HistoryChrominos[IndexMove].square1);
        AnimateSquare('#' + HistoryChrominos[IndexMove].square1);
        ShowSquare('#' + HistoryChrominos[IndexMove].square2);
        AnimateSquare('#' + HistoryChrominos[IndexMove].square2);
    }
    $('#PlayerHistoryPseudo').html(HistoryChrominos[IndexMove].playerName).fadeIn().delay(1000).fadeOut();
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
let TimeoutPut = null;
let ToPut = false;
let PositionLastChromino;
let TimeoutRotate = null;
let ToRotate = true;
let LastChrominoMove = null;
let OffsetLastChromino = 0;
let OffsetGameArea = null;

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
                if (IsChrominoInGameArea(this)) {
                    $('#ButtonPlayChromino').show();
                    ScheduleValidateChromino();
                }
                else {
                    $('#ButtonPlayChromino').hide();
                }
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
                    PlayChromino();
                }
                ToPut = false;
            });
    }
}

function StopDraggable() {
    if (Tips.find(x => x.elementId == "Hand") == undefined) {
        $(this).off("mouseup");
        $(this).off("mousedown");
        $(document).off("mousemove");
        $('.handPlayerChromino').draggableTouch("disable");
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
let GameAreaOffsetX;
let GameAreaOffsetY;
let SquareSize;

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
    let offsetRight = 0.5 * SquareSize;
    let offsetBottom = 0.5 * SquareSize;
    let offsetLeft, offsetTop;
    if (GetOrientation(chromino) == "horizontal") {
        offsetLeft = 2.5 * SquareSize;
        offsetTop = 0.5 * SquareSize;
    }
    else {
        offsetLeft = 0.5 * SquareSize;
        offsetTop = 2.5 * SquareSize;
    }
    let heightGameArea = $('#GameArea').height();
    let widthGameArea = $('#GameArea').width();
    let offsetLastChromino = $(chromino).offset();
    if (offsetLastChromino.left + offsetLeft > OffsetGameArea.left && offsetLastChromino.left + offsetRight < OffsetGameArea.left + widthGameArea && offsetLastChromino.top + offsetTop > OffsetGameArea.top && offsetLastChromino.top + offsetBottom < OffsetGameArea.top + heightGameArea)
        return true;
    else
        return false;
}

//***************************************//
//******* fonction PlayChromino *********//
//***************************************//
function PlayChromino() {
    if (!ThisPlayerTurn) {
        $('#errorMessage').html("Vous devez attendre votre tour avant de jouer !");
        $('#errorMessageEnd').html("Merci");
        ShowPopup('#errorPopup');
        return;
    }
    if (LastChrominoMove != null) {
        let offset = $(LastChrominoMove).offset();
        let xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        let yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
        switch (GetAngle(LastChrominoMove)) {
            case 0:
                $("#FormOrientation").val(Horizontal);
                $("#FormFlip").val(false);
                break;
            case 90:
                $("#FormOrientation").val(Vertical);
                $("#FormFlip").val(false);
                yIndex--;
                break;
            case 180:
                $("#FormOrientation").val(Horizontal);
                $("#FormFlip").val(true);
                break;
            case 270:
                $("#FormOrientation").val(Vertical);
                $("#FormFlip").val(true);
                yIndex--;
                break;
            default:
                break;
        }
        $("#FormX").val(xIndex + XMin);
        $("#FormY").val(yIndex + YMin);
        $("#FormChrominoId").val(LastChrominoMove.id);
        $("#FormPlayChromino").submit();
    }
    else {
        $('#errorMessage').html("Vous devez poser un chromino dans le jeu");
        ShowPopup('#errorPopup');
    }
}

//***************************************************//
//********** gestion popups de la partie ************//
//***************************************************//

function ShowPopup(popup, hasCloseButton = true) {
    StopScheduleValidateChromino();
    $(popup).show();
    $(popup).popup({ closebutton: hasCloseButton, autoopen: true, transition: 'all 0.4s' });
    $.fn.popup.defaults.pagecontainer = '#page';
    if (popup == '#ChatPopup') {
        $('#ChatPopup-textarea').scrollTop(300);
        let timeoutScroll;
        clearTimeout(timeoutScroll);
        timeoutScroll = setTimeout(function () {
            $('#ChatPopup-textarea').scrollTop($('#ChatPopup-textarea')[0].scrollHeight);
        }, 50);
        if (NotReadMessages > 0) {
            $('#ChatPopup-Read').show();
        }
    }
}

function ClosePopup(popup) {
    $(popup).popup('hide');
}

let TimeoutValidateChromino = null;
function ScheduleValidateChromino() {
    clearTimeout(TimeoutValidateChromino);
    TimeoutValidateChromino = setTimeout(function () {
        Action("ValidateChromino");
    }, 4000);

}
function StopScheduleValidateChromino() {
    clearTimeout(TimeoutValidateChromino);
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
    //$('#TipId').val(Tip.id);
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
var Tip;
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
