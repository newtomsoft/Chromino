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
    if (NotReadMessages != 0) {
        $('#NotifChat').show();
        $('#NotifChat').text(NotReadMessages);
    }

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
    if (popupId == '#ChatPopup') {
        let textArea = document.getElementById('ChatPopup-textarea');
        //textArea.scrollTop = textArea.scrollHeight;
        textArea.scrollTop = 500;
        if (NotReadMessages > 0) {
            $('#ChatPopup-Read').show();
        }
    }
    $(popupId).show();
    $(popupId).popup({
        autoopen: true,
        transition: 'all 0.4s'
    });
    $.fn.popup.defaults.pagecontainer = '#page'
}

function HidePopup(popupId) {
    $(popupId).hide();
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
        position = $(LastChrominoMove).offset();
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

function StartDraggable() {
    $(".handPlayerChromino").draggableTouch()
        .on("dragstart", function () {
            $(this).css('cursor', 'grabbing');
            ScheduleRotate();
            LastChrominoMove = this;
            PositionLastChromino = $(LastChrominoMove).offset();
            SchedulePut();
        }).on("dragend", function () {
            $(this).css('cursor', 'grab');
            LastChrominoMove = this;
            MagnetChromino();
            if (ToRotate) {
                ToRotate = false;
                clearTimeout(TimeoutRotate);
                Rotation(LastChrominoMove);
            }
            var position = $(LastChrominoMove).offset();
            if (ToPut && PositionLastChromino.left == position.left && PositionLastChromino.top == position.top) {
                clearTimeout(TimeoutPut);
                PutChromino();
            }
            ToPut = false;
        });
}

function Rotation(chromino) {
    var transform = $(chromino).css("transform");
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
    if (LastChrominoMove != null) {
        var offset = $(LastChrominoMove).offset();
        var x = offset.left - GameAreaOffsetX;
        var y = offset.top - GameAreaOffsetY;
        var difX = SquareSize * Math.round(x / SquareSize) - x;
        var difY = SquareSize * Math.round(y / SquareSize) - y;
        $(LastChrominoMove).css({ "left": "+=" + difX + "px", "top": "+=" + difY + "px" });
    }
}

function PutChromino() {
    if (!ThisPlayerTurn) {
        $('#errorMessage').html("Vous devez attendre votre tour avant de jouer !");
        $('#errorMessageEnd').html("Merci");
        ShowErrorPopup();
        return;
    }
    if (LastChrominoMove != null) {
        var id = LastChrominoMove.id;
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
        var offset = $(LastChrominoMove).offset();
        var xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        var yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
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
            $(this).css("transform", "matrix(1, 0, 0, 1, 0, 0)");
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
    }
    else {
        height -= 200;
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").each(function () {
            $(this).css({ left: offset });
            $(this).css({ top: documentHeight - SquareSize * 3 }); // marge d'1 SquareSize en bas implicite par la rotation (matrix)
            $(this).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
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
}
