

function continueRandomGame() {
    location.assign("@Url.Action("ContinueRandomGame", "Game", new { id = ViewBag.GameId })");
}