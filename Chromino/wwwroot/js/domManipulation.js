function RefreshDom(opponentPlay) {
    if (IsBot)
        PlayBot(PlayerTurnId);

    if (opponentPlay) {
        AnimateChrominosPlayed(true);
    }

    ResizeGameArea();
    StartDraggable();

    //animatation des derniers chromino joués
    if (!PreviouslyDraw && ThisPlayerTurn) {
        AnimateChrominosPlayed(0);
    }

    // affichage notif nombre de messages non lus
    if (NotReadMessages != 0) {
        $('#NotifChat').text(NotReadMessages);
        $('#NotifChat').show();
    }

    // affichage notif nombres de mémos
    if (MemosNumber != 0) {
        $('#NotifMemo').text(MemosNumber);
        $('#NotifMemo').show();
    }

    // affichage notif help
    //if (Model.Player.Help > 0 && !Model.ShowPossiblesPositions && !Model.Game.Status.IsFinish())
    if (HelpNumber != 0) {
        $('#HelpNumber').text(HelpNumber);
        $('#ButtonHelp').show();
    }
    // affichage des Squares Possible
    $('.Square').removeClass('Possible');
    if (HelpIndexes.length > 0) {
        let squaresSelected = HelpIndexes.map(i => '#Square_' + i).join(", ");
        $(squaresSelected).addClass('Possible');
        HelpIndexes = new Array;
    }

    // affichage popup
    if (ShowInfoPopup)
        ShowPopup('#PopupInfo');
    else if (ShowBotPlayingInfoPopup)
        ShowPopup('#botPlayingInfoPopup');
}


function AddChrominoInHand(chromino) {
    let divToAdd = `<div id="${chromino.id}" class="handPlayerChromino">`;
    for (let i = 0; i < 3; i++)
        divToAdd += `<div class="Square ${chromino.colors[i]}"></div>`;
    divToAdd += '</div>'
    $(divToAdd).appendTo('#Hand');
    ResizeGameArea();
}

function AddChrominoInGame(chromino, playerName) {
    offset = chromino.orientation == Horizontal ? { x: 1, y: 0 } : { x: 0, y: 1 };
    let squaresName = new Array;
    for (var iSquare = 0; iSquare < 3; iSquare++) {
        x = chromino.xIndex + offset.x * iSquare;
        y = chromino.yIndex + offset.y * iSquare;
        if (y > GameAreaLinesNumber - 3) {
            AddGameLineEnd();
        }
        if (x > GameAreaColumnsNumber - 3) {
            AddGameColumnEnd();
        }
        index = x + y * GameAreaColumnsNumber;
        squareName = "Square_" + index;
        squaresName.push(squareName);
        squareSelector = "#" + squareName;
        switch (iSquare) {
            case 0:
                classColor = chromino.flip ? chromino.colors[2] : chromino.colors[0];
                classOpenSides = chromino.orientation == Horizontal ? "Square OpenRight" : "Square OpenBottom";
                break;
            case 1:
                classColor = chromino.colors[1];
                classOpenSides = chromino.orientation == Horizontal ? "Square OpenRightLeft" : "Square OpenBottomTop";
                break;
            case 2:
                classColor = chromino.flip ? chromino.colors[0] : chromino.colors[2];
                classOpenSides = chromino.orientation == Horizontal ? "Square OpenLeft" : "Square OpenTop";
                break;
        }
        $(squareSelector).removeClass().addClass(classOpenSides + " " + classColor);
    }
    HistoryChrominos.splice(0, 0, { playerName: playerName, square0: squaresName[0], square1: squaresName[1], square2: squaresName[2] });
    ResizeGameArea();
}

function AddHistorySkipTurn(playerName) {
    HistoryChrominos.splice(0, 0, { playerName: playerName + (playerName == "Vous" ? " avez" : " a") + " passé" });

}

function AddGameLineEnd() {
    let firstIndexToAdd = GameAreaLinesNumber * GameAreaColumnsNumber;
    let divToAdd = `<div id="Line_${GameAreaLinesNumber}" class="gameLineArea">`;
    for (let i = 0; i < GameAreaColumnsNumber; i++)
        divToAdd += `<div id="Square_${firstIndexToAdd + i}" class="Square Free"></div>`;
    divToAdd += '</div>'
    $(divToAdd).appendTo('#GameArea');
    GameAreaLinesNumber++;
}

function AddGameColumnEnd() {
    $("div[id^='Line_']").each(function (i) {
        let squareNumber = (GameAreaColumnsNumber + 1) * (i + 1) - 1;
        let divToAdd = `<div id="Square_${squareNumber}" class="Square Free"></div>`;
        $(divToAdd).appendTo(this);
        let lineSelector = "#Line_" + (i + 1);
        $(lineSelector).children(".Square").each(function (j) {
            this.id = "Square_" + (squareNumber + j + 1);
        });
    });
    GameAreaColumnsNumber++;
}

function RemoveChrominoInHand(chrominoId) {
    $('#' + chrominoId).remove();
}
function ShowButtonNextGame() {
    $('#ButtonNextGame').show();
}
function HideButtonNextGame() {
    $('#ButtonNextGame').hide();
}
function ShowButtonSkipTurn() {
    $('#ButtonSkipTurn').show();
}
function HideButtonSkipTurn() {
    $('#ButtonSkipTurn').hide();
}
function ShowButtonDrawChromino() {
    $('#ButtonDrawChromino').show();
}
function HideButtonDrawChromino() {
    $('#ButtonDrawChromino').hide();
}
function ShowButtonPlayChromino() {
    $('#ButtonPlayChromino').show();
}
function HideButtonPlayChromino() {
    $('#ButtonPlayChromino').hide();
}