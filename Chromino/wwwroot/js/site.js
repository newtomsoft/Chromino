$(document).ready(function () {

    // Action StartNew events
    $('#addPlayer').click(function () {
        AddPlayer();
    });
    $('#removePlayer').click(function () {
        RemovePlayer();
    });


    //********************//
    //*** Actions Game ***//
    //********************//
    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    $(window).bind('resize', function (e) {
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
//******** gestion popup infos de la partie *********//
//***************************************************//
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

function StartDraggable() {
    $(".handPlayerChromino").draggableTouch().bind("dragstart", function () {
        ScheduleRotate();
        LastChrominoMove = this;
    }).bind("dragend", function () {
        LastChrominoMove = this;
        if (ToRotate) {
            ToRotate = false;
            clearTimeout(TimeoutRotate);
            Rotation(LastChrominoMove)
        }
        else {
            ToRotate = false;
        }
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
        case "matrix(-1, 0, 0, -1, 0, 0)": //180° => 270°
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
    $(this).unbind("mouseup");
    $(this).unbind("mousedown");
    $(document).unbind("mousemove");
    $(".handPlayerChromino").draggableTouch("disable");
}

function PutChromino() {
    var id = LastChrominoMove.id;
    var transform = $(LastChrominoMove).css("transform");
    switch (transform) {
        case "none":
        case "matrix(1, 0, 0, 1, 0, 0)": // 0°
            $("#FormOrientation").val(1); // todo : valeurs scalaire à changer en conformité avec l'enum c#
            break;
        case "matrix(0, 1, -1, 0, 0, 0)": // 90°
            $("#FormOrientation").val(4);
            break;
        case "matrix(-1, 0, 0, -1, 0, 0)": //180°
            $("#FormOrientation").val(3);
            break;
        case "matrix(0, -1, 1, 0, 0, 0)": // 270°
            $("#FormOrientation").val(2);
            break;
        default:
            break;
    }
    var position = $(LastChrominoMove).position();
    var x = position.left - GameAreaOffsetX;
    var y = position.top - GameAreaOffsetY;
    var xIndex = Math.round(x / SquareSize);
    var yIndex = Math.round(y / SquareSize);

    $("#FormX").val(xIndex + XMin);
    $("#FormY").val(yIndex + YMin);
    $("#FormChrominoId").val(id);
    $("#FormSendMove").submit();
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

    GameAreaOffsetX = (documentWidth - gameAreaWidth) / 2;
    GameAreaOffsetY = (documentHeight - gameAreaHeight) / 2;

    $('.gameLineArea').outerHeight("auto");
    $('.Square').outerHeight(SquareSize);
    $('.Square').outerWidth(SquareSize);
    $('.handPlayerChromino').outerHeight(SquareSize);
    $('#gameArea').show();
    $('.gameLineArea').css('display', 'flex');
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