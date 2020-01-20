$(document).ready(function () {

    // Action StartNew events
    $('#addPlayer').click(function () {
        addPlayer();
    });
    $('#removePlayer').click(function () {
        removePlayer();
    });

    resizeGameArea();

});


//***************************************//
//** gestion déplacement des chrominos **//
//***************************************//

$(".handPlayerChromino")
    .draggableTouch()
    .bind("dragstart", function (event, pos) {
        //console.log("drag started on:", this, "at position:", pos);
        $("#chromino0position").html("left : " + pos.left + "top : " + pos.top);
    })
    .bind("dragend", function (event, pos) {
        $("#chromino0position").html("left : " + pos.left + "top : " + pos.top);
    });



//***************************************//
//********* fonctions StartNew  *********//
//***************************************//

function addPlayer() {
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
function removePlayer() {
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

function resizeGameArea() {
    var linesNumber = parseInt($('#gameAreaLinesNumber').html()) + 4;
    var columnNumber = parseInt($('#gameAreaColumnsNumber').html()) + 4;

    var width = $(document).width();
    var height = $(document).height();
    if (width < height) {
        height = height - 200;
    }
    else {
        width = width - 200;
    }
    var squareHeight = height / linesNumber;
    var squareWidth = width / columnNumber;

    var squareMin = Math.min(Math.trunc(Math.min(squareHeight, squareWidth)), 30);

    $('.gameLineArea').outerHeight("auto");

    $('.squareOpenRight').outerWidth(squareMin);
    $('.squareOpenBottom').outerWidth(squareMin);
    $('.squareOpenLeft').outerWidth(squareMin);
    $('.squareOpenTop').outerWidth(squareMin);
    $('.squareOpenTopBotom').outerWidth(squareMin);
    $('.squareOpenLeftRight').outerWidth(squareMin);
    $('.squareFreeCloseNone').outerWidth(squareMin);
    $('.squareFreeCloseTop').outerWidth(squareMin);
    $('.squareFreeCloseRightTop').outerWidth(squareMin);
    $('.squareFreeCloseTopBotom').outerWidth(squareMin);
    $('.squareFreeCloseRightBottomTop').outerWidth(squareMin);
    $('.squareFreeCloseLeftTop').outerWidth(squareMin);
    $('.squareFreeCloseLeftRight').outerWidth(squareMin);
    $('.squareFreeCloseBottomLeftTop').outerWidth(squareMin);
    $('.squareFreeCloseAll').outerWidth(squareMin);
    $('.squareFreeCloseRightLeftTop').outerWidth(squareMin);
    $('.squareFreeCloseRight').outerWidth(squareMin);
    $('.squareFreeCloseBottom').outerWidth(squareMin);
    $('.squareFreeCloseRightBottom').outerWidth(squareMin);
    $('.squareFreeCloseLeft').outerWidth(squareMin);
    $('.squareFreeCloseBottomLeft').outerWidth(squareMin);
    $('.squareFreeCloseRightBottomLeft').outerWidth(squareMin);
    $('.squareFreeCloseRightLeft').outerWidth(squareMin);

    $('.squareOpenRight').outerHeight(squareMin);
    $('.squareOpenBottom').outerHeight(squareMin);
    $('.squareOpenLeft').outerHeight(squareMin);
    $('.squareOpenTop').outerHeight(squareMin);
    $('.squareOpenTopBotom').outerHeight(squareMin);
    $('.squareOpenLeftRight').outerHeight(squareMin);
    $('.squareFreeCloseNone').outerHeight(squareMin);
    $('.squareFreeCloseTop').outerHeight(squareMin);
    $('.squareFreeCloseRightTop').outerHeight(squareMin);
    $('.squareFreeCloseTopBotom').outerHeight(squareMin);
    $('.squareFreeCloseRightBottomTop').outerHeight(squareMin);
    $('.squareFreeCloseLeftTop').outerHeight(squareMin);
    $('.squareFreeCloseLeftRight').outerHeight(squareMin);
    $('.squareFreeCloseBottomLeftTop').outerHeight(squareMin);
    $('.squareFreeCloseAll').outerHeight(squareMin);
    $('.squareFreeCloseRightLeftTop').outerHeight(squareMin);
    $('.squareFreeCloseRight').outerHeight(squareMin);
    $('.squareFreeCloseBottom').outerHeight(squareMin);
    $('.squareFreeCloseRightBottom').outerHeight(squareMin);
    $('.squareFreeCloseLeft').outerHeight(squareMin);
    $('.squareFreeCloseBottomLeft').outerHeight(squareMin);
    $('.squareFreeCloseRightBottomLeft').outerHeight(squareMin);
    $('.squareFreeCloseRightLeft').outerHeight(squareMin);

    $('#gameArea').show();
    $('.gameLineArea').css('display', 'flex');
}



//let XBegin, XEnd, XDiff, YBegin, YEnd, YDiff;
//let ChominoPosition;
//let IsMoving = false;

//function TouchStart(chromino, typeEvent) {
//    IsMoving = true;
//    ChominoPosition = $(chromino).position();

//    XDiff = 0;
//    YDiff = 0;

//    if (typeEvent == "mouse") {
//        XBegin = event.x;
//        YBegin = event.y;
//    }
//    else {
//        XBegin = event.touches[0].clientX;
//        YBegin = event.touches[0].clientY;
//    }
//}

//function TouchMove(chromino, typeEvent) {
//    if (IsMoving) {
//        if (typeEvent == "mouse") {
//            XEnd = event.x;
//            YEnd = event.y;
//        }
//        else {
//            XEnd = event.touches[0].clientX;
//            YEnd = event.touches[0].clientY;
//        }
//        XDiff = XEnd - XBegin;
//        YDiff = YEnd - YBegin;
//        chromino.style.transform = 'translate(' + XDiff + 'px, ' + YDiff + 'px)';
//    }
//}

//function TouchEnd(chromino) {
//    chromino.style.transform = 'none';
//    var left = ChominoPosition.left + XDiff;
//    var top = ChominoPosition.top + YDiff;
//    $(chromino).offset({ top: top, left: left });
//    IsMoving = false;
//}