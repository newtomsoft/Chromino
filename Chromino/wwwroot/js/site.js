$(document).ready(function () {

    // Action StartNew events
    $('#addPlayer').click(function () {
        AddPlayer();
    });
    $('#removePlayer').click(function () {
        RemovePlayer();
    });
    ShowPlayers();

    //********************//
    //*** Event on Game **//
    //********************//
    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    $(document).mouseup(function () {
        MagnetChromino();
    });

    $(window).on('resize', function (e) {
        window.resizeEvt;
        $(window).resize(function () {
            clearTimeout(window.resizeEvt);
            window.resizeEvt = setTimeout(function () {
                ResizeGameArea();
            }, 250);
        });
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
});

//***************************************************//
//**** gestion affichage derniers chrominos joués ***//
//***************************************************//
var IndexMove = 0;
function AnimateChrominosPlayed() {
    index = IndexMove * 3;
    for (var i = index; i < index + 3; i++) {
        AnimateSquare('#' + Squares[i]);
    }
    $('#PlayerHistory').html(Pseudos[IndexMove]).fadeIn(300).delay(400).fadeOut(300);
}
function AnimateSquare(squareId) {
    $(squareId).fadeToggle(500, function () {
        $(this).fadeToggle(500);
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
let PositionLastChromino;
let TimeoutRotate = null;
let ToRotate = true;
let LastChrominoMove;

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
            ScheduleRotate();
            LastChrominoMove = this;
            PositionLastChromino = $(LastChrominoMove).offset();
            SchedulePut();
        }).on("dragend", function () {
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
    var offset = $(LastChrominoMove).offset();
    var x = offset.left - GameAreaOffsetX;
    var y = offset.top - GameAreaOffsetY;
    var difX = SquareSize * Math.round(x / SquareSize) - x;
    var difY = SquareSize * Math.round(y / SquareSize) - y;
    $(LastChrominoMove).css({ "left": "+=" + difX + "px", "top": "+=" + difY + "px" });
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
            $(this).css({ left: documentWidth - 100 });
            $(this).css({ top: offset });
            $(this).css("transform", "matrix(1, 0, 0, 1, 0, 0)");
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
    }
    else {
        height -= 200;
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        FirstResizeGameArea = false;
        $(".handPlayerChromino").each(function () {
            $(this).css({ left: offset });
            $(this).css({ top: documentHeight - 100 });
            $(this).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
            offset += SquareSize + Math.floor(SquareSize / 10);
        });
    }
    var gameAreaHeight = SquareSize * GameAreaLinesNumber;
    var gameAreaWidth = SquareSize * GameAreaColumnsNumber;
    $('#gameArea').height(gameAreaHeight);
    $('#gameArea').width(gameAreaWidth);
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

//***************************************//
//********* fonctions StartNew  *********//
//***************************************//
function AddPlayer() {
    if ($('#groupPlayer2').is(':hidden'))
        $('#groupPlayer2').show(600);
    else if ($('#groupPlayer3').is(':hidden'))
        $('#groupPlayer3').show(600);
    else if ($('#groupPlayer4').is(':hidden'))
        $('#groupPlayer4').show(600);
    else if ($('#groupPlayer5').is(':hidden'))
        $('#groupPlayer5').show(600);
    else if ($('#groupPlayer6').is(':hidden'))
        $('#groupPlayer6').show(600);
    else if ($('#groupPlayer7').is(':hidden'))
        $('#groupPlayer7').show(600);
    else if ($('#groupPlayer8').is(':hidden'))
        $('#groupPlayer8').show(600);
}
function RemovePlayer() {
    if (!$('#groupPlayer8').is(':hidden')) {
        $('#player8').val('');
        $('#groupPlayer8').hide(600);
    }
    else if (!$('#groupPlayer7').is(':hidden')) {
        $('#player7').val('');
        $('#groupPlayer7').hide(600);
    }
    else if (!$('#groupPlayer6').is(':hidden')) {
        $('#player6').val('');
        $('#groupPlayer6').hide(600);
    }
    else if (!$('#groupPlayer5').is(':hidden')) {
        $('#player5').val('');
        $('#groupPlayer5').hide(600);
    }
    else if (!$('#groupPlayer4').is(':hidden')) {
        $('#player4').val('');
        $('#groupPlayer4').hide(600);
    }
    else if (!$('#groupPlayer3').is(':hidden')) {
        $('#player3').val('');
        $('#groupPlayer3').hide(600);
    }
    else if (!$('#groupPlayer2').is(':hidden')) {
        $('#player2').val('');
        $('#groupPlayer2').hide(600);
    }
}
function ShowPlayers() {
    $(".playerName").each(function (index, element) {
        if ($(element).val() != '')
            $(element).parent().show(600);
    });
}