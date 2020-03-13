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

    //animate lasts chrominos played
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

    // désactivation du menu contextuel
    window.oncontextmenu = function (event) {
        event.preventDefault();
        event.stopPropagation();
    };
});

//***************************************************//
//**** gestion affichage derniers chrominos joués ***//
//***************************************************//
let IndexMove = 0;
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
function ShowErrorPopup() {
    $('#errorPopup').show();
    $('#errorPopup').popup({
        autoopen: true,
        transition: 'all 0.4s'
    });
    $.fn.popup.defaults.pagecontainer = '#page'
}

function ShowInfoPopup() {
    $('#infoPopup').show();
    $('#infoPopup').popup({
        autoopen: true,
        transition: 'all 0.4s'
    });
    $.fn.popup.defaults.pagecontainer = '#page'
}
function HideInfoPopup() {
    $('#infoPopup').hide();
}


//***************************************************//
//** gestion déplacements / rotation des chrominos **//
//***************************************************//
let TimeoutPut = null;
let ToPut = false;
let OffsetLastChromino;
let TimeoutRotate = null;
let ToRotate = true;
let LastChrominoMove = null;
let OffsetHand = null;
let OffsetGameArea = null;
let HeightGameArea = null;
let WidthGameArea = null;

function ScheduleRotate() {
    ToRotate = true;
    clearTimeout(TimeoutRotate);
    TimeoutRotate = setTimeout(function () {
        ToRotate = false;
    }, 120);
}

function SchedulePut() {
    clearTimeout(TimeoutPut);
    OffsetLastChromino = $(LastChrominoMove).offset();
    TimeoutPut = setTimeout(function () {
        offset = $(LastChrominoMove).offset();
        if (OffsetLastChromino.left == offset.left && OffsetLastChromino.top == offset.top) {
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

function StartDraggable() {
    $(".handPlayerChromino").draggableTouch()
        .on("dragstart", function () {
            $(this).css('cursor', 'grabbing');
            ScheduleRotate();
            LastChrominoMove = this;
            OffsetLastChromino = $(this).offset();
            if ($(this).css('position') != "fixed") {
                $(this).css('position', 'fixed');
                $(this).offset(OffsetLastChromino);
                $("#" + $(this).attr('id') + "-hidden").show();
            }
            SchedulePut();
        }).on("dragend", function () {
            $(this).css('cursor', 'grab');
            LastChrominoMove = this;
            OffsetLastChromino = $(this).offset();
            if (OffsetLastChromino.left < OffsetHand.left || OffsetLastChromino.top < OffsetHand.top) {
                $("#" + $(this).attr('id') + "-hidden").hide();
            }
            MagnetChromino();
            if (ToRotate) {
                ToRotate = false;
                clearTimeout(TimeoutRotate);
                Rotation(LastChrominoMove);
            }
            let offset = $(LastChrominoMove).offset();
            if (ToPut && OffsetLastChromino.left == offset.left && OffsetLastChromino.top == offset.top) {
                clearTimeout(TimeoutPut);
                PutChromino();
            }
            ToPut = false;
        });
}

function Rotation(chromino) {
    let transform = $(chromino).css("transform");
    switch (transform) {
        case "none":
        case "matrix(1, 0, 0, 1, 0, 0)": // 0° => 90°
            $(chromino).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
            break;
        case "matrix(0, 1, -1, 0, 0, 0)": // 90° => 180°
            $(chromino).css("transform", "matrix(-1, 0, 0, -1, 0, 0)");
            break;
        case "matrix(-1, 0, 0, -1, 0, 0)": // 180° => 270°
            $(chromino).css("transform", "matrix(0, -1, 1, 0, 0, 0)");
            break;
        case "matrix(0, -1, 1, 0, 0, 0)": // 270° => 0°
            $(chromino).css("transform", "matrix(1, 0, 0, 1, 0, 0)");
            break;
        default:
            break;
    }
}

function StopDraggable() {
    $(this).off("mouseup");
    $(this).off("mousedown");
    $(document).off("mousemove");
    $('.handPlayerChromino').draggableTouch("disable");
}

function MagnetChromino() {
    if (LastChrominoMove != null && IsChrominoInGameArea()) {
        let offset = $(LastChrominoMove).offset();
        let x = offset.left - GameAreaOffsetX;
        let y = offset.top - GameAreaOffsetY;
        let difX = SquareSize * Math.round(x / SquareSize) - x;
        let difY = SquareSize * Math.round(y / SquareSize) - y;
        $(LastChrominoMove).css({ "left": "+=" + difX + "px", "top": "+=" + difY + "px" });
    }
}

function IsChrominoInGameArea() {
    let offset = SquareSize * 2;
    if (OffsetLastChromino.left + offset >= OffsetGameArea.left && OffsetLastChromino.left <= OffsetGameArea.left + WidthGameArea && OffsetLastChromino.top + offset >= OffsetGameArea.top && OffsetLastChromino.top <= OffsetGameArea.top + HeightGameArea)
        return true;
    else
        return false;
}

function PutChromino() {
    if (!ThisPlayerTurn) {
        $('#errorMessage').html("Vous devez attendre votre tour avant de jouer !");
        $('#errorMessageEnd').html("Merci");
        ShowErrorPopup();
        return;
    }
    if (LastChrominoMove != null) {
        let id = LastChrominoMove.id;
        switch ($(LastChrominoMove).css("transform")) {
            case "none":
            case "matrix(1, 0, 0, 1, 0, 0)":
                $("#FormOrientation").val(Horizontal);
                break;
            case "matrix(0, 1, -1, 0, 0, 0)":
                $("#FormOrientation").val(VerticalFlip);
                break;
            case "matrix(-1, 0, 0, -1, 0, 0)":
                $("#FormOrientation").val(HorizontalFlip);
                break;
            case "matrix(0, -1, 1, 0, 0, 0)":
                $("#FormOrientation").val(Vertical);
                break;
            default:
                break;
        }
        let offset = $(LastChrominoMove).offset();
        let xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        let yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
        $("#FormX").val(xIndex + XMin);
        $("#FormY").val(yIndex + YMin);
        $("#FormChrominoId").val(id);
        $("#FormSendMove").submit();
    }
    else {
        $('#errorMessage').html("Vous devez poser un chromino dans le jeu");
        ShowErrorPopup();
    }
}

//***************************************************//
//**** gestion drag chromino en dehors/ dans main ***//
//***************************************************//

function hideSpace(event) {
    //alert("le chromino quitte la main");
    index = IndexMove * 3;
    let id = event.fromElement.id;
    for (iflash = 0; iflash < 3; iflash++) {
        AnimateSquare('#' + id);
    }
}

function showSpace(event) {
    //alert("le chromino entre dans la main");

}

function computeOrder(event) {
    //alert("le chromino est déposé dans la main");
}


//***************************************//
//********* fonctions GameArea  *********//
//***************************************//
let GameAreaOffsetX;
let GameAreaOffsetY;
let SquareSize;

function ResizeGameArea() {
    let documentWidth = $(document).width();
    let documentHeight = $(document).height();
    let width = documentWidth;
    let height = documentHeight;
    if (width > height) {
        width -= 160; //-160 : somme de la taille des 2 bandeaux
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").each(function () {
            $(this).css("transform", "matrix(1, 0, 0, 1, 0, 0)");
            $(this).width(SquareSize * 3);
            $(this).height(SquareSize);
        });
    }
    else {
        height -= 160;
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").each(function () {
            $(this).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
            $(this).width(SquareSize);
            $(this).height(SquareSize * 3);
        });
    }
    $('#gameArea').height(SquareSize * GameAreaLinesNumber);
    $('#gameArea').width(SquareSize * GameAreaColumnsNumber);
    $('.gameLineArea').outerHeight("auto");
    $('.Square').outerHeight(SquareSize);
    $('.Square').outerWidth(SquareSize);
    $('.gameLineArea').css('display', 'flex');
    let gameAreaOffset = $('#gameArea').offset();
    GameAreaOffsetX = gameAreaOffset.left;
    GameAreaOffsetY = gameAreaOffset.top;

    OffsetHand = $('#hand').offset();
    OffsetGameArea = $('#gameArea').offset();
    HeightGameArea = $('#gameArea').height();
    WidthGameArea = $('#gameArea').width();
}
