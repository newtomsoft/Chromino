function RefreshDom(opponentPlay) {
    if (IsBot)
        PlayBot(PlayerTurnId);

    RefreshOpponentChromino(opponentPlay);

    //if (!PreviouslyDraw && ThisPlayerTurn) {
    //    AnimateChromino(2, true);
    //}

    RefreshButtonChat();
    RefreshButtonMemo();
    RefreshHelp();
    RefreshInfoPopup();
    if (ShowInfoPopup)
        ShowPopup('#PopupInfo');
    //else if (ShowBotPlayingInfoPopup)
    //    ShowPopup('#botPlayingInfoPopup');

    ResizeGameArea();
    StopDraggable();
    StartDraggable();
    if (IsGameFinish) {
        HideButtonDrawChromino();
        HideButtonPlayChromino();
        HideButtonSkipTurn();
        ShowButtonNextGame();
        StopDraggable();
    }

}

function RefreshInfoPopup() {
    if (IsGameFinish) {
        if (Players.find(p => p.id == PlayerId).chrominosNumber != 0) {
            html = "<h2>Vous avez perdu</h2><h3>Dommage</h3><br />";
        }
        else if (Players.findIndex(p => p.id != PlayerId && p.chrominosNumber == 0) != -1) {
            html = "<h2>Victoire ex-æquo </h2><h3>Bravo</h3><br />";
        }
        else {
            html = "<h2>Victoire</h2><h3>Bravo !</h3><br />";
        }
        $('#PopupInfoHead').html(html);
    }
    else {
        $('#PopupInfoHead').html(`<h3>${PlayerTurnText}</h3>`);
    }
    RefreshInStack();
}

function RefreshInStack() {
    switch (InStack) {
        case 0:
            $('#InStack').html("Il n'y a plus de chrominos dans la pioche");
            break;
        case 1:
            $('#InStack').html('Pioche : 1 chromino');
            break
        default:
            $('#InStack').html(`Pioche : ${InStack} chrominos`);
            break;
    }
}

function UpdateInHandNumberDom(player) {
    let playerHave = player.name == "Vous" ? "Vous avez" : player.name + " a";
    $('#Player_' + player.id).removeClass();
    switch (player.chrominosNumber) {
        case 0:
            $('#Player_' + player.id).addClass("winner");
            $('#Player_' + player.id).html(`${playerHave} gagné la partie`);
            break;
        case 1:
            $('#Player_' + player.id).addClass("opponentLastChromino");
            let divToAdd = playerHave + '&nbsp';
            for (let i = 0; i < 3; i++) {
                let classOpenSides;
                switch (i) {
                    case 0:
                        classOpenSides = "OpenRight";
                        break;
                    case 1:
                        classOpenSides = "OpenRightLeft";
                        break;
                    case 2:
                        classOpenSides = "OpenLeft";
                        break;
                }
                divToAdd += `<div class="Square ${classOpenSides} ${player.lastChrominoColors[i]}"></div>`;
            }
            $('#Player_' + player.id).html(divToAdd);
            break;
        default:
            $('#Player_' + player.id).html(`${playerHave} ${player.chrominosNumber} chrominos en main`);
            break
    }
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
    if (chromino.yIndex == GameAreaLinesNumber - 2 && chromino.orientation == Horizontal || chromino.yIndex == GameAreaLinesNumber - 4 && chromino.orientation == Vertical)
        AddGameLineBottom(1);
    else if (chromino.yIndex == GameAreaLinesNumber - 3 && chromino.orientation == Vertical)
        AddGameLineBottom(2);
    else if (chromino.yIndex < 2) {
        AddGameLineTop(2 - chromino.yIndex);
        chromino.yIndex = 2;
    }
    if (chromino.xIndex == GameAreaColumnsNumber - 2 && chromino.orientation == Vertical || chromino.xIndex == GameAreaColumnsNumber - 4 && chromino.orientation == Horizontal)
        AddGameColumnRight(1);
    else if (chromino.xIndex == GameAreaColumnsNumber - 3 && chromino.orientation == Horizontal)
        AddGameColumnRight(2);
    else if (chromino.xIndex < 2) {
        AddGameColumnLeft(2 - chromino.xIndex);
        chromino.xIndex = 2;
    }
    let squaresName = new Array;
    offset = chromino.orientation == Horizontal ? { x: 1, y: 0 } : { x: 0, y: 1 };
    for (var iSquare = 0; iSquare < 3; iSquare++) {
        index = chromino.xIndex + offset.x * iSquare + (chromino.yIndex + offset.y * iSquare) * GameAreaColumnsNumber;
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
    ShowLastChrominoPlayed();
    ResizeGameArea();
}

function AddHistorySkipTurn(playerName) {
    HistoryChrominos.splice(0, 0, { playerName: playerName + (playerName == "Vous" ? " avez" : " a") + " passé" });
}

function AddGameLineBottom(add) {
    for (let iadd = 0; iadd < add; iadd++) {
        let firstIndexToAdd = GameAreaLinesNumber * GameAreaColumnsNumber;
        let divToAdd = `<div id="Line_${GameAreaLinesNumber}" class="gameLineArea">`;
        for (let i = 0; i < GameAreaColumnsNumber; i++)
            divToAdd += `<div id="Square_${firstIndexToAdd + i}" class="Square Free"></div>`;
        divToAdd += '</div>'
        $(divToAdd).appendTo('#GameArea');
        GameAreaLinesNumber++;
    }
}

function AddGameLineTop(add) {
    for (let iadd = 0; iadd < add; iadd++) {
        $("div[id^='Line_']").each(function (i) {
            this.id = "Line_" + (i + 1);
        });
        $(".Square").each(function (i) {
            this.id = "Square_" + (GameAreaColumnsNumber + i);
        });
        let divToAdd = `<div id="Line_0" class="gameLineArea">`;
        for (let i = 0; i < GameAreaColumnsNumber; i++)
            divToAdd += `<div id="Square_${i}" class="Square Free"></div>`;
        divToAdd += '</div>'
        $('#GameArea').prepend(divToAdd);
        GameAreaLinesNumber++;
        YMin--;
    }
    HistoryChrominos.UpdateSquares(add, GameAreaColumnsNumber, 'top');
}

function AddGameColumnLeft(add) {
    let oldColumnsNumber = GameAreaColumnsNumber;
    for (let iadd = 0; iadd < add; iadd++) {
        $("div[id^='Line_']").each(function (i) {
            let squareNumber = (GameAreaColumnsNumber + 1) * i;
            let divToAdd = `<div id="Square_${squareNumber}" class="Square Free"></div>`;
            let lineSelector = "#Line_" + i;
            $(lineSelector).children(".Square").each(function (j) {
                this.id = "Square_" + (squareNumber + j + 1);
            });
            $(this).prepend(divToAdd);
        });
        GameAreaColumnsNumber++;
        XMin--;
    }
    HistoryChrominos.UpdateSquares(add, oldColumnsNumber, 'left');
}

function AddGameColumnRight(add) {
    let oldColumnsNumber = GameAreaColumnsNumber;
    for (let iadd = 0; iadd < add; iadd++) {
        $("div[id^='Line_']").each(function (i) {
            let squareNumber = i * (GameAreaColumnsNumber + 1) + GameAreaColumnsNumber;
            let divToAdd = `<div id="Square_${squareNumber}" class="Square Free"></div>`;
            $(divToAdd).appendTo(this);
            let lineSelector = "#Line_" + (i + 1);
            $(lineSelector).children(".Square").each(function (j) {
                this.id = "Square_" + (squareNumber + j + 1);
            });
        });
        GameAreaColumnsNumber++;
    }

    HistoryChrominos.UpdateSquares(add, oldColumnsNumber, 'right');
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

function RefreshButtonHelp() {
    if (HelpNumber != 0 && !IsGameFinish) {
        $('#HelpNumber').text(HelpNumber);
        $('#ButtonHelp').show();
    }
    else
        $('#ButtonHelp').hide();
}
function RefreshButtonMemo() {
    if (MemosNumber != 0) {
        $('#NotifMemo').text(MemosNumber);
        $('#NotifMemo').show();
    }
    else
        $('#NotifMemo').hide();
}
function RefreshButtonChat() {
    if (NotReadMessages != 0) {
        $('#NotifChat').text(NotReadMessages);
        $('#NotifChat').show();
    }
    else
        $('#NotifChat').hide();
}
function RefreshHelp() {
    $('.Square').removeClass('Possible');
    if (HelpIndexes.length > 0) {
        let squares = HelpIndexes.map(i => '#Square_' + i).join(", ");
        $(squares).addClass('Possible');
        HelpIndexes = new Array;
    }
    RefreshButtonHelp();
}
function RefreshOpponentChromino(opponentPlay) {
    if (opponentPlay) {
        AnimateChromino(3, true, true);
    }
}
