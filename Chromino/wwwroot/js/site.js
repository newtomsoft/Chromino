$(document).ready(function () {

    // Action New events
    $('#addPlayer').click(function () {
        AddPlayer();
    });
    $('#removePlayer').click(function () {
        RemovePlayer();
    });
    ShowPlayers();
});


//****************************************//
//********* fonctions New player *********//
//****************************************//
function AddPlayer() {
    if ($('#groupPlayer3').is(':hidden')) {
        $('#player3').prop('required', true);
        $('#groupPlayer3').show(600);
    }
    else if ($('#groupPlayer4').is(':hidden')) {
        $('#player4').prop('required', true);
        $('#groupPlayer4').show(600);
    }
    else if ($('#groupPlayer5').is(':hidden')) {
        $('#player5').prop('required', true);
        $('#groupPlayer5').show(600);
    }
    else if ($('#groupPlayer6').is(':hidden')) {
        $('#player6').prop('required', true);
        $('#groupPlayer6').show(600);
    }
    else if ($('#groupPlayer7').is(':hidden')) {
        $('#player7').prop('required', true);
        $('#groupPlayer7').show(600);
    }
    else if ($('#groupPlayer8').is(':hidden')) {
        $('#player8').prop('required', true);
        $('#groupPlayer8').show(600);
    }
}
function RemovePlayer() {
    if (!$('#groupPlayer8').is(':hidden')) {
        $('#player8').prop('required', false);
        $('#player8').val('');
        $('#groupPlayer8').hide(600);
    }
    else if (!$('#groupPlayer7').is(':hidden')) {
        $('#player7').prop('required', false);
        $('#player7').val('');
        $('#groupPlayer7').hide(600);
    }
    else if (!$('#groupPlayer6').is(':hidden')) {
        $('#player6').prop('required', false);
        $('#player6').val('');
        $('#groupPlayer6').hide(600);
    }
    else if (!$('#groupPlayer5').is(':hidden')) {
        $('#player5').prop('required', false);
        $('#player5').val('');
        $('#groupPlayer5').hide(600);
    }
    else if (!$('#groupPlayer4').is(':hidden')) {
        $('#player4').prop('required', false);
        $('#player4').val('');
        $('#groupPlayer4').hide(600);
    }
    else if (!$('#groupPlayer3').is(':hidden')) {
        $('#player3').prop('required', false);
        $('#player3').val('');
        $('#groupPlayer3').hide(600);
    }
}
function ShowPlayers() {
    $(".playerName").each(function (index, element) {
        if ($(element).val() != '')
            $(element).parent().show(600);
    });
}