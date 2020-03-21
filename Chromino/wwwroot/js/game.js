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

    //animatation des derniers chromino joués
    if (!PreviouslyDraw && ThisPlayerTurn) {
        AnimateChrominosPlayed(0);
    }
    $("#previousButton").click(function () {
        if (IndexMove < Squares.length / 3 - 1) {
            IndexMove++;
        }
        AnimateChrominosPlayed();
    });
    $("#nextButton").click(function () {
        if (IndexMove > 0) {
            IndexMove--;
        }
        AnimateChrominosPlayed();
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

    // affichage popup
    if (PlayReturn != "Ok")
        ShowPopup('#errorPopup');
    else if (ShowInfoPopup)
        ShowPopup('#infoPopup');
    else if (ShowBotPlayingInfoPopup)
        ShowPopup('#botPlayingInfoPopup');

});

//***************************************************//
//**** gestion affichage derniers chrominos joués ***//
//***************************************************//
var IndexMove = 0;
function AnimateChrominosPlayed() {
    index = IndexMove * 3;
    for (i = index; i < index + 3; i++) {
        for (iflash = 0; iflash < 3; iflash++) {
            AnimateSquare('#' + Squares[i]);
        }
    }
    $('#PlayerHistoryPseudo').html(Pseudos[IndexMove]).fadeIn().delay(1000).fadeOut();
}
function AnimateSquare(squareId) {
    $(squareId).fadeToggle(150, function () {
        $(this).fadeToggle(150);
    });
}


//***************************************************//
//********** gestion popups de la partie ************//
//***************************************************//

function ShowPopup(popupId) {
    $(popupId).show();
    $(popupId).popup({
        autoopen: true,
        transition: 'all 0.4s'
    });
    $.fn.popup.defaults.pagecontainer = '#page';
    if (popupId == '#ChatPopup') {

        $('#ChatPopup-textarea').scrollTop(300);
        let TimeoutScroll;
        clearTimeout(TimeoutScroll);
        TimeoutScroll = setTimeout(function () {
            $('#ChatPopup-textarea').scrollTop($('#ChatPopup-textarea')[0].scrollHeight);
        }, 50);
        if (NotReadMessages > 0) {
            $('#ChatPopup-Read').show();
        }
    }
}

//***************************************************//
//*** information de valider position pour jouer ****//
//***************************************************//
let TimeoutInformPlaying = null;
function ScheduleInformPlaying() {
    clearTimeout(TimeoutInformPlaying);
    TimeoutInformPlaying = setTimeout(function () {
        ShowPopup('#playingInfoPopup');
    }, 9000);
}
function StopScheduleInformPlaying() {
    clearTimeout(TimeoutInformPlaying);
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
    $(".handPlayerChromino").draggableTouch()
        .on("dragstart", function () {
            $(this).css('cursor', 'grabbing');
            ScheduleRotate();
            LastChrominoMove = this;
            PositionLastChromino = $(LastChrominoMove).offset();
            SchedulePut();
            StopScheduleInformPlaying();
        }).on("dragend", function () {
            if (IsChrominoInGameArea(this)) {
                $('.btn-play').show();
                ScheduleInformPlaying();
            }
            else {
                $('.btn-play').hide();
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

function StopDraggable() {
    $(this).off("mouseup");
    $(this).off("mousedown");
    $(document).off("mousemove");
    $('.handPlayerChromino').draggableTouch("disable");
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
    $('#gameArea').fadeToggle(25, function () {
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

function Rotation(chromino, angle = 90) {
    let newAngle = angle + GetAngle(chromino);
    if (newAngle >= 360)
        newAngle -= 360;
    SetAngle(chromino, newAngle);
    let orientation = GetOrientation(chromino);
    if (angle == 90 || angle == -90) {
        if (orientation == "horizontal")
            SetOffset(chromino, -SquareSize, 0);
        else
            SetOffset(chromino, SquareSize, 0);
    }
}

function SetOffset(chromino, left, top) {
    let offset = $(chromino).offset();
    let newLeft = offset.left + left;
    let newTop = offset.top + top;
    $(chromino).offset({ top: newTop, left: newLeft });
}

function SetGoodOrientation(chromino) {
    if (Landscape)
        SetHorizontal(chromino);
    else
        SetVertical(chromino);
}

function SetHorizontal(chromino) {
    let rotation = GetAngle(chromino);
    if (rotation == 90)
        SetAngle(chromino, 180);
    else if (rotation == 270)
        SetAngle(chromino, 0);
}

function SetVertical(chromino) {
    let rotation = GetAngle(chromino);
    if (rotation == 0)
        SetAngle(chromino, 90);
    else if (rotation == 180)
        SetAngle(chromino, 270);
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
    switch (rotate) {
        case 0:
            $(chromino).css("flex-direction", "row");
            break;
        case 90:
            $(chromino).css("flex-direction", "column");
            break;
        case 180:
            $(chromino).css("flex-direction", "row-reverse");
            break;
        case 270:
            $(chromino).css("flex-direction", "column-reverse");
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
        $(".handPlayerChromino").each(function () {
            $(this).css({ left: documentWidth - SquareSize * 4 }); //3 SquareSize pour le chromino + 1 de marge à droite
            $(this).css({ top: offset });
            SetAngle(this, 0);
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
    }
    else {
        height -= 200;
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").each(function () {
            $(this).css({ left: offset });
            $(this).css({ top: documentHeight - SquareSize * 3 }); // marge d'1 SquareSize en bas implicite par la rotation (matrix)
            SetAngle(this, 90);
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
    }
    $('#gameArea').height(SquareSize * GameAreaLinesNumber);
    $('#gameArea').width(SquareSize * GameAreaColumnsNumber);
    $('.gameLineArea').outerHeight("auto");
    $('.Square').outerHeight(SquareSize);
    $('.Square').outerWidth(SquareSize);
    $('.handPlayerChromino').outerHeight(SquareSize);
    $('#gameArea').show();
    $('.gameLineArea').css('display', 'flex');
    var gameAreaOffset = $('#gameArea').offset();
    GameAreaOffsetX = gameAreaOffset.left;
    GameAreaOffsetY = gameAreaOffset.top;
    OffsetGameArea = $('#gameArea').offset();
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
    let heightGameArea = $('#gameArea').height();
    let widthGameArea = $('#gameArea').width();
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
                break;
            case 90:
                $("#FormOrientation").val(VerticalFlip);
                yIndex--;
                break;
            case 180:
                $("#FormOrientation").val(HorizontalFlip);
                break;
            case 270:
                $("#FormOrientation").val(Vertical);
                yIndex--;
                break;
            default:
                break;
        }
        $("#FormX").val(xIndex + XMin);
        $("#FormY").val(yIndex + YMin);
        $("#FormChrominoId").val(LastChrominoMove.id);
        $("#FormSendMove").submit();
    }
    else {
        $('#errorMessage').html("Vous devez poser un chromino dans le jeu");
        ShowPopup('#errorPopup');
    }
}