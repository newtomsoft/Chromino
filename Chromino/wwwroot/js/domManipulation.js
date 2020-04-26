function RefreshDom(opponentPlay) {
    if (IsBot)
        PlayBot(PlayerTurnId);

    if (opponentPlay) {
        AnimateChrominosPlayed(true);
    }

    ResizeGameArea();
    StopDraggable();
    StartDraggable();

    if (!PreviouslyDraw && ThisPlayerTurn) {
        AnimateChrominosPlayed(0);
    }

    if (NotReadMessages != 0) {
        $('#NotifChat').text(NotReadMessages);
        $('#NotifChat').show();
    }

    if (MemosNumber != 0) {
        $('#NotifMemo').text(MemosNumber);
        $('#NotifMemo').show();
    }

    if (HelpNumber != 0 && !IsGameFinish) {
        $('#HelpNumber').text(HelpNumber);
        $('#ButtonHelp').show();
    }

    $('.Square').removeClass('Possible');
    if (HelpIndexes.length > 0) {
        let squaresSelected = HelpIndexes.map(i => '#Square_' + i).join(", ");
        $(squaresSelected).addClass('Possible');
        HelpIndexes = new Array;
    }

    RefreshInfoPopup();
    if (ShowInfoPopup)
        ShowPopup('#PopupInfo');
    else if (ShowBotPlayingInfoPopup)
        ShowPopup('#botPlayingInfoPopup');

    RefreshInStack();

    if (IsGameFinish) {
        HideButtonDrawChromino();
        HideButtonPlayChromino();
        HideButtonSkipTurn();
        ShowButtonNextGame();
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
    offset = chromino.orientation == Horizontal ? { x: 1, y: 0 } : { x: 0, y: 1 };
    let squaresName = new Array;
    for (var iSquare = 0; iSquare < 3; iSquare++) {
        x = chromino.xIndex + offset.x * iSquare;
        y = chromino.yIndex + offset.y * iSquare;
        if (y > GameAreaLinesNumber - 3) {
            if (chromino.orientation == Vertical && iSquare == 1)
                AddGameLineBottom(2);
            else
                AddGameLineBottom(1);
        }
        else if (y < 2) {
            AddGameLineTop(2 - y);
            chromino.yIndex = 2;
            y = 2;
        }
        if (x == GameAreaColumnsNumber - 2 && chromino.orientation == Vertical || x == GameAreaColumnsNumber - 4 && chromino.orientation == Horizontal)
            AddGameColumnRight(1);
        else if (x == GameAreaColumnsNumber - 3 && chromino.orientation == Horizontal)
            AddGameColumnRight(2);
        else if (x < 2) {
            AddGameColumnLeft(2 - x);
            chromino.xIndex = 2;
            x = 2;
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

function AddGameLineBottom() {
    let firstIndexToAdd = GameAreaLinesNumber * GameAreaColumnsNumber;
    let divToAdd = `<div id="Line_${GameAreaLinesNumber}" class="gameLineArea">`;
    for (let i = 0; i < GameAreaColumnsNumber; i++)
        divToAdd += `<div id="Square_${firstIndexToAdd + i}" class="Square Free"></div>`;
    divToAdd += '</div>'
    $(divToAdd).appendTo('#GameArea');
    GameAreaLinesNumber++;
}

function AddGameLineTop(add) {
    for (let i = 0; i < add; i++) {
        $("div[id^='Line_']").each(function (i) {
            this.id = "Line_" + (i + 1);
        });
        $(".Square").each(function (j) {
            this.id = "Square_" + (GameAreaColumnsNumber + j);
        });
        let divToAdd = `<div id="Line_0" class="gameLineArea">`;
        for (let i = 0; i < GameAreaColumnsNumber; i++)
            divToAdd += `<div id="Square_${i}" class="Square Free"></div>`;
        divToAdd += '</div>'
        $('#GameArea').prepend(divToAdd);
    }
    let increase = add * GameAreaColumnsNumber;
    HistoryChrominos.forEach(function (item) {
        newNumber = parseInt(item.square0.replace("Square_", "")) + increase;
        item.square0 = "Square_" + newNumber;
        newNumber = parseInt(item.square1.replace("Square_", "")) + increase;
        item.square1 = "Square_" + newNumber;
        newNumber = parseInt(item.square2.replace("Square_", "")) + increase;
        item.square2 = "Square_" + newNumber;
    });
    GameAreaLinesNumber++;
    YMin--;
}

function AddGameColumnLeft(add) {
    for (let i = 0; i < add; i++) {
        $("div[id^='Line_']").each(function (i) {
            let squareNumber = (GameAreaColumnsNumber + 1) * i;
            let divToAdd = `<div id="Square_${squareNumber}" class="Square Free"></div>`;
            let lineSelector = "#Line_" + i;
            $(lineSelector).children(".Square").each(function (j) {
                newId = "Square_" + (squareNumber + j + 1);
                squarefound = HistoryChrominos.find(c => c.square0 == this.id);
                if (squarefound !== undefined)
                    squarefound.square0 = newId;
                else {
                    squarefound = HistoryChrominos.find(c => c.square1 == this.id);
                    if (squarefound !== undefined)
                        squarefound.square1 = newId;
                    else {
                        squarefound = HistoryChrominos.find(c => c.square2 == this.id);
                        if (squarefound !== undefined)
                            squarefound.square2 = newId;
                    }
                }
                this.id = newId
            });
            $(this).prepend(divToAdd);
        });
        GameAreaColumnsNumber++;
        XMin--;
    }
}

function AddGameColumnRight(add) {
    for (let iAdd = 0; iAdd < add; iAdd++) {
        $("div[id^='Line_']").each(function (i) {
            let squareNumber = i * (GameAreaColumnsNumber + 1) + GameAreaColumnsNumber;
            let divToAdd = `<div id="Square_${squareNumber}" class="Square Free"></div>`;
            $(divToAdd).appendTo(this);
            let lineSelector = "#Line_" + (i + 1);
            $(lineSelector).children(".Square").each(function (j) {
                this.id = "Square_" + (squareNumber + j + 1);
            });
        });
    }
    HistoryChrominos.forEach(function (item) {
        oldNumber = parseInt(item.square0.replace("Square_", ""));
        item.square0 = "Square_" + (oldNumber + add * Math.floor(oldNumber / GameAreaColumnsNumber));
        oldNumber = parseInt(item.square1.replace("Square_", ""));
        item.square1 = "Square_" + (oldNumber + add * Math.floor(oldNumber / GameAreaColumnsNumber));
        oldNumber = parseInt(item.square2.replace("Square_", ""));
        item.square2 = "Square_" + (oldNumber + add * Math.floor(oldNumber / GameAreaColumnsNumber));
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