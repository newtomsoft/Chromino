function GetGuids() {
    $.ajax({
        url: '/Games/Guids',
        success: function (data) { CallbackGetGuids(data); },
    });
}

function AgainstFriends() {
    $.ajax({
        url: '/Games/AgainstFriendsAjax',
        success: function (data) { CallbackAgainstFriends(data); }
    });
}

function AgainstBots() {

}

function WithUnreadMessages() {

}

function Singles() {

}

function FinishWithFriends() {

}

function FinishWithBotd() {

}