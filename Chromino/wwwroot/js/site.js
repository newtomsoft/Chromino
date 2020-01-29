$(document).ready(function () {

    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    // Action StartNew events
    $('#addPlayer').click(function () {
        AddPlayer();
    });
    $('#removePlayer').click(function () {
        RemovePlayer();
    });

    ResizeGameArea();
    StartDraggable();
});


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
    var position = $(LastChrominoMove).position();
    var x = position.left - GameAreaOffsetX;
    var y = position.top - GameAreaOffsetY;
    var xIndex = Math.round(x / SquareSize);
    var yIndex = Math.round(y / SquareSize);

    $("#FormX").val(xIndex + XMin);
    $("#FormY").val(yIndex + YMin);
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
    $("#FormChrominoId").val(id);
    $("#FormSendMove").submit();
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
    if (width > height) {
        width -= 200; //-200 : taille bandeaux
        //placement des chrominos de la main ok
    }
    else {
        height -= 200;
        //rotation des chrominos de la main du joueur et changement de place     
        $(".handPlayerChromino").each(function () {
            var position = $(this).position();
            $(this).css({ left: position.top });
            $(this).css({ top: documentHeight - 80 });
            //$(this).css({ bottom: '30px' });
            $(this).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
        });


    }
    SquareSize = Math.min(Math.trunc(Math.min(height / gameAreaLinesNumber, width / gameAreaColumnsNumber)), 30);

    var gameAreaHeight = SquareSize * gameAreaLinesNumber;
    var gameAreaWidth = SquareSize * gameAreaColumnsNumber;
    $('#gameArea').height(gameAreaHeight);
    $('#gameArea').width(gameAreaWidth);

    GameAreaOffsetX = (documentWidth - gameAreaWidth) / 2 - 10; //todo documentWidth pas le bon ; +10 offset inconnu
    GameAreaOffsetY = (documentHeight - gameAreaHeight) / 2 - 10;

    $('.gameLineArea').outerHeight("auto");
    $('.OpenRight').outerWidth(SquareSize);
    $('.OpenBottom').outerWidth(SquareSize);
    $('.OpenLeft').outerWidth(SquareSize);
    $('.OpenTop').outerWidth(SquareSize);
    $('.OpenBottomTop').outerWidth(SquareSize);
    $('.OpenRightLeft').outerWidth(SquareSize);
    $('.CloseNone').outerWidth(SquareSize);
    $('.CloseTop').outerWidth(SquareSize);
    $('.CloseRightTop').outerWidth(SquareSize);
    $('.CloseBottomTop').outerWidth(SquareSize);
    $('.CloseRightBottomTop').outerWidth(SquareSize);
    $('.CloseLeftTop').outerWidth(SquareSize);
    $('.CloseLeftRight').outerWidth(SquareSize);
    $('.CloseBottomLeftTop').outerWidth(SquareSize);
    $('.CloseAll').outerWidth(SquareSize);
    $('.CloseRightLeftTop').outerWidth(SquareSize);
    $('.CloseRight').outerWidth(SquareSize);
    $('.CloseBottom').outerWidth(SquareSize);
    $('.CloseRightBottom').outerWidth(SquareSize);
    $('.CloseLeft').outerWidth(SquareSize);
    $('.CloseBottomLeft').outerWidth(SquareSize);
    $('.CloseRightBottomLeft').outerWidth(SquareSize);
    $('.CloseRightLeft').outerWidth(SquareSize);

    $('.OpenRight').outerHeight(SquareSize);
    $('.OpenBottom').outerHeight(SquareSize);
    $('.OpenLeft').outerHeight(SquareSize);
    $('.OpenTop').outerHeight(SquareSize);
    $('.OpenBottomTop').outerHeight(SquareSize);
    $('.OpenRightLeft').outerHeight(SquareSize);
    $('.CloseNone').outerHeight(SquareSize);
    $('.CloseTop').outerHeight(SquareSize);
    $('.CloseRightTop').outerHeight(SquareSize);
    $('.CloseBottomTop').outerHeight(SquareSize);
    $('.CloseRightBottomTop').outerHeight(SquareSize);
    $('.CloseLeftTop').outerHeight(SquareSize);
    $('.CloseLeftRight').outerHeight(SquareSize);
    $('.CloseBottomLeftTop').outerHeight(SquareSize);
    $('.CloseAll').outerHeight(SquareSize);
    $('.CloseRightLeftTop').outerHeight(SquareSize);
    $('.CloseRight').outerHeight(SquareSize);
    $('.CloseBottom').outerHeight(SquareSize);
    $('.CloseRightBottom').outerHeight(SquareSize);
    $('.CloseLeft').outerHeight(SquareSize);
    $('.CloseBottomLeft').outerHeight(SquareSize);
    $('.CloseRightBottomLeft').outerHeight(SquareSize);
    $('.CloseRightLeft').outerHeight(SquareSize);

    $('#gameArea').show();
    $('.gameLineArea').css('display', 'flex');
}
