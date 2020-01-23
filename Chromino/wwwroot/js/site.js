$(document).ready(function () {

    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    $(".handPlayerChromino").dblclick(function () {
        PutChromino(this);
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

jQuery.fn.rotate = function (degrees) {
    $(this).css({ 'transform': 'rotate(' + degrees + 'deg)' });
};

let TimeoutRotate = null;
let ToRotate = true;
let IsPut;

function ScheduleRotate() {
    ToRotate = true;
    clearTimeout(TimeoutRotate);
    TimeoutRotate = setTimeout(function () {
        ToRotate = false;
    }, 120);
}

function StartDraggable() {
    $(".handPlayerChromino")
        .draggableTouch()
        .bind("dragstart", function (event, pos) {
            ScheduleRotate();
            var id = this.id;
            var x = pos.left - GameAreaOffsetX;
            var y = pos.top - GameAreaOffsetY;
            $("#chrominoPosition").html("position " + id + " left : " + x + "top : " + y);
        })
        .bind("dragend", function (event, pos) {
            if (ToRotate) {
                ToRotate = false;
                clearTimeout(TimeoutRotate);
                var chromino = this;
                clearTimeout(TimeoutPut);
                IsPut = false;
                var TimeoutPut = setTimeout(function () {
                    Rotation(chromino);
                }, 300);

            }
            else {
                var id = this.id;
                var x = pos.left - GameAreaOffsetX;
                var y = pos.top - GameAreaOffsetY;
                $("#chrominoPosition").html("position " + id + " left : " + x + "top : " + y);
                clearTimeout(TimeoutRotate);
                ToRotate = false;
            }
        });
}

function Rotation(chromino) {
    if (!IsPut) {
        var transform = $(chromino).css("transform");
        switch (transform) {
            case "none":
            case "matrix(1, 0, 0, 1, 0, 0)": // 0° => 90°
                $(chromino).rotate(90);
                break;
            case "matrix(0, 1, -1, 0, 0, 0)": // 90° => 180°
                $(chromino).rotate(180);
                break;
            case "matrix(-1, 0, 0, -1, 0, 0)": //180° => 270°
                $(chromino).rotate(270);
                break;
            case "matrix(0, -1, 1, 0, 0, 0)": // 270° => 0°
                $(chromino).rotate(0);
                break;
            default:
                break;
        }
    }
}

function StopDraggable() {
    $(this).unbind("mouseup");
    $(this).unbind("mousedown");
    $(document).unbind("mousemove");
    $(".handPlayerChromino").draggableTouch("disable");
}

function PutChromino(chromino) {
    IsPut = true;
    var id = chromino.id;
    var position = $(chromino).position();
    var x = position.left - GameAreaOffsetX;
    var y = position.top - GameAreaOffsetY;
    $("#chrominoPosition").html(id + "put at position left : " + x + "top : " + y);
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
var GameAreaOffsetX;
var GameAreaOffsetY;

function ResizeGameArea() {
    var documentWidth = $(document).width();
    var documentHeight = $(document).height();
    var width = documentWidth;
    var height = documentHeight;
    if (width < height) {
        height = height - 200; //-200 : taille bandeaux
    }
    else {
        width = width - 200;
    }
    var squareSize = Math.min(Math.trunc(Math.min(height / gameAreaLinesNumber, width / gameAreaColumnsNumber)), 30);

    var gameAreaHeight = squareSize * gameAreaLinesNumber;
    var gameAreaWidth = squareSize * gameAreaColumnsNumber;
    $('#gameArea').height(gameAreaHeight);
    $('#gameArea').width(gameAreaWidth);

    GameAreaOffsetX = (documentWidth - gameAreaWidth) / 2;
    GameAreaOffsetY = (documentHeight - gameAreaHeight) / 2;

    $('.gameLineArea').outerHeight("auto");
    $('.squareOpenRight').outerWidth(squareSize);
    $('.squareOpenBottom').outerWidth(squareSize);
    $('.squareOpenLeft').outerWidth(squareSize);
    $('.squareOpenTop').outerWidth(squareSize);
    $('.squareOpenTopBotom').outerWidth(squareSize);
    $('.squareOpenLeftRight').outerWidth(squareSize);
    $('.squareFreeCloseNone').outerWidth(squareSize);
    $('.squareFreeCloseTop').outerWidth(squareSize);
    $('.squareFreeCloseRightTop').outerWidth(squareSize);
    $('.squareFreeCloseTopBotom').outerWidth(squareSize);
    $('.squareFreeCloseRightBottomTop').outerWidth(squareSize);
    $('.squareFreeCloseLeftTop').outerWidth(squareSize);
    $('.squareFreeCloseLeftRight').outerWidth(squareSize);
    $('.squareFreeCloseBottomLeftTop').outerWidth(squareSize);
    $('.squareFreeCloseAll').outerWidth(squareSize);
    $('.squareFreeCloseRightLeftTop').outerWidth(squareSize);
    $('.squareFreeCloseRight').outerWidth(squareSize);
    $('.squareFreeCloseBottom').outerWidth(squareSize);
    $('.squareFreeCloseRightBottom').outerWidth(squareSize);
    $('.squareFreeCloseLeft').outerWidth(squareSize);
    $('.squareFreeCloseBottomLeft').outerWidth(squareSize);
    $('.squareFreeCloseRightBottomLeft').outerWidth(squareSize);
    $('.squareFreeCloseRightLeft').outerWidth(squareSize);
    $('.squareOpenRight').outerHeight(squareSize);
    $('.squareOpenBottom').outerHeight(squareSize);
    $('.squareOpenLeft').outerHeight(squareSize);
    $('.squareOpenTop').outerHeight(squareSize);
    $('.squareOpenTopBotom').outerHeight(squareSize);
    $('.squareOpenLeftRight').outerHeight(squareSize);
    $('.squareFreeCloseNone').outerHeight(squareSize);
    $('.squareFreeCloseTop').outerHeight(squareSize);
    $('.squareFreeCloseRightTop').outerHeight(squareSize);
    $('.squareFreeCloseTopBotom').outerHeight(squareSize);
    $('.squareFreeCloseRightBottomTop').outerHeight(squareSize);
    $('.squareFreeCloseLeftTop').outerHeight(squareSize);
    $('.squareFreeCloseLeftRight').outerHeight(squareSize);
    $('.squareFreeCloseBottomLeftTop').outerHeight(squareSize);
    $('.squareFreeCloseAll').outerHeight(squareSize);
    $('.squareFreeCloseRightLeftTop').outerHeight(squareSize);
    $('.squareFreeCloseRight').outerHeight(squareSize);
    $('.squareFreeCloseBottom').outerHeight(squareSize);
    $('.squareFreeCloseRightBottom').outerHeight(squareSize);
    $('.squareFreeCloseLeft').outerHeight(squareSize);
    $('.squareFreeCloseBottomLeft').outerHeight(squareSize);
    $('.squareFreeCloseRightBottomLeft').outerHeight(squareSize);
    $('.squareFreeCloseRightLeft').outerHeight(squareSize);
    $('#gameArea').show();
    $('.gameLineArea').css('display', 'flex');
}
