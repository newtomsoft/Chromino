function RefreshDom() {
    if (IsBot)
        PlayBot(PlayerTurnId);

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

function AddChrominoInGame(xIndex, yIndex, orientation, flip, colors) {
    offset = orientation == Horizontal ? { x: 1, y: 0 } : { x: 0, y: 1 };
    for (var iSquare = 0; iSquare < 3; iSquare++) {
        i = xIndex + offset.x * iSquare;
        j = yIndex + offset.y * iSquare;
        index = i + j * GameAreaColumnsNumber;
        squareSelector = "#Square_" + index;
        if (iSquare == 0) {
            classColor = flip ? colors[2] : colors[0];
            classOpenSides = orientation == Horizontal ? "Square OpenRight" : "Square OpenBottom";
        }
        else if (iSquare == 1) {
            classColor = colors[1];
            classOpenSides = orientation == Horizontal ? "Square OpenRightLeft" : "Square OpenBottomTop";
        }
        else {
            classColor = flip ? colors[0] : colors[2];
            classOpenSides = orientation == Horizontal ? "Square OpenLeft" : "Square OpenTop";
        }

        $(squareSelector).removeClass().addClass(classOpenSides + " " + classColor);
    }
    ResizeGameArea();
}

function RefreshGameArea(squaresVM) {
    //todo
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