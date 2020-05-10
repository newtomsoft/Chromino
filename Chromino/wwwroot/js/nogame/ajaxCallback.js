function CallbackGetGuids(data) {
    Guids = data.guids;
    CallSignalR();
}

function CallbackAgainstFriends(data) {
    ShowGamesAgainstFriendsNumber(data.picturesGame.length);
}





function ShowGamesAgainstFriendsNumber(number) {
    $("#AgainstFriendsNumber").text(number);
    $("#AgainstFriendsNumber").show();
}