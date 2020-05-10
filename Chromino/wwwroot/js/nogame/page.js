var Guids;
var GamesAgainstFriendsNumber;
var GamesAgainstBotsNumber;
var GamesWithUnreadMessagesNumber;
var GamesSinglesNumber;
var GamesFinishWithFriendsNumber;
var GamesFinishWithBotdNumber;

$(document).ready(function () {
    AddFeatureNewGame();
    GetGuids();
    if (IndexGamesLoad)
        GetGames();
});

function GetGames() {
    AgainstFriends();
    AgainstBots();
    WithUnreadMessages();
    Singles();
    FinishWithFriends();
    FinishWithBotd();
}
