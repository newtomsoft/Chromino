$(document).ready(function () {

    // Action StartNew events
    $('#addPlayer').click(function () {
        addPlayer();
    });
    $('#removePlayer').click(function () {
        removePlayer();
    });

    //
    //resizeGameArea();


});


// Action StartNew functions
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

    var squareMin = Math.trunc(Math.min(squareHeight, squareWidth));


    $('.gameLineArea').outerHeight("auto");
    $('.squareOpenBottomLeftTop').outerWidth(squareMin);
    $('.squareOpenBottomLeftTop').outerHeight(squareMin);
    $('.squareOpenRightLeftTop').outerWidth(squareMin);
    $('.squareOpenRightLeftTop').outerHeight(squareMin);
    $('.squareOpenLeftTop').outerWidth(squareMin);
    $('.squareOpenLeftTop').outerHeight(squareMin);
    $('.squareOpenBottomLeft').outerWidth(squareMin);
    $('.squareOpenBottomLeft').outerHeight(squareMin);
    $('.squareOpenRightTop').outerWidth(squareMin);
    $('.squareOpenRightTop').outerHeight(squareMin);
    $('.squareOpenAll').outerWidth(squareMin);
    $('.squareOpenAll').outerHeight(squareMin);
    $('.squareOpenRight').outerWidth(squareMin);
    $('.squareOpenRight').outerHeight(squareMin);
    $('.squareOpenBottom').outerWidth(squareMin);
    $('.squareOpenBottom').outerHeight(squareMin);
    $('.squareOpenLeft').outerWidth(squareMin);
    $('.squareOpenLeft').outerHeight(squareMin);
    $('.squareOpenTop').outerWidth(squareMin);
    $('.squareOpenTop').outerHeight(squareMin);
    $('.squareOpenTopBotom').outerWidth(squareMin);
    $('.squareOpenTopBotom').outerHeight(squareMin);
    $('.squareOpenLeftRight').outerWidth(squareMin);
    $('.squareOpenLeftRight').outerHeight(squareMin);
}
