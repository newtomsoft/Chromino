function RefreshDom(opponentPlayed) {
    if (HaveBotResponsability())
        PlayingBot(PlayerTurn.id);
    StopScheduleValidateChromino();
    if (opponentPlayed)
        ShowHistoryLatestMove();
    RefreshButtonNextGame();
    RefreshButtonMemo();
    RefreshButtonsDrawSkip();
    RefreshHelp();
    RefreshInfoPopup();
    ResizeGameArea();
    StopDraggable();
    StartDraggable();
    if (IsGameFinish) {
        HideButtonPlayChromino();
        RefreshButtonNextGame();
        StopDraggable();
    }
}

function RefreshInfoPopup() {
    if (IsGameFinish) {
        if (Players.find(p => p.id == PlayerId) !== undefined) {
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
    }
    else {
        let infoHead = PlayerTurn.id == PlayerId ? "C'est à vous de jouer" : `C'est à ${PlayerTurn.name} de jouer`;
        $('#PopupInfoHead').html(`<h3>${infoHead}</h3>`);
    }
    RefreshInStack();
    if (ShowInfoPopup && PlayerTurn.id == PlayerId || IsGameFinish)
        ShowPopup('#PopupInfo', true);
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
    let spanPlayerId = `<span player-id='${player.id}' class='penpal-status'></span>`;
    let playerName = player.id == PlayerId ? "Vous" : player.name;
    let playerWithStatus = player.id != PlayerId && !player.isBot ? `<span run='Chat ${player.id}'>${playerName}${spanPlayerId}</span>` : playerName;
    let have = player.id == PlayerId ? "&nbsp;avez" : "&nbsp;a";
    $(`#Player_${player.id}`).removeClass();
    switch (player.chrominosNumber) {
        case 0:
            $(`#Player_${player.id}`).addClass("winner");
            $(`#Player_${player.id}`).html(`${playerWithStatus}${have} terminé`);
            break;
        case 1:
            $(`#Player_${player.id}`).addClass("opponentLastChromino");
            let divToAdd = `${playerWithStatus}${have} &nbsp`;
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
            $(`#Player_${player.id}`).html(divToAdd);
            break;
        default:
            $(`#Player_${player.id}`).html(`${playerWithStatus}${have} ${player.chrominosNumber} chrominos en main`);
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

function AddChrominoInGame(chromino, infoPlayerPlay) {
    let chrominoWork = chromino;
    if (chrominoWork.yIndex == GameAreaLinesNumber - 2 && chrominoWork.orientation == Orientation.horizontal || chrominoWork.yIndex == GameAreaLinesNumber - 4 && chrominoWork.orientation == Orientation.vertical)
        AddGameLineBottom(1);
    else if (chrominoWork.yIndex == GameAreaLinesNumber - 3 && chrominoWork.orientation == Orientation.vertical)
        AddGameLineBottom(2);
    else if (chrominoWork.yIndex < 2) {
        AddGameLineTop(2 - chrominoWork.yIndex);
        chrominoWork.yIndex = 2;
    }
    if (chrominoWork.xIndex == GameAreaColumnsNumber - 2 && chrominoWork.orientation == Orientation.vertical || chrominoWork.xIndex == GameAreaColumnsNumber - 4 && chrominoWork.orientation == Orientation.horizontal)
        AddGameColumnRight(1);
    else if (chrominoWork.xIndex == GameAreaColumnsNumber - 3 && chrominoWork.orientation == Orientation.horizontal)
        AddGameColumnRight(2);
    else if (chrominoWork.xIndex < 2) {
        AddGameColumnLeft(2 - chrominoWork.xIndex);
        chrominoWork.xIndex = 2;
    }
    let squaresName = new Array;
    offset = chrominoWork.orientation == Orientation.horizontal ? { x: 1, y: 0 } : { x: 0, y: 1 };
    for (var iSquare = 0; iSquare < 3; iSquare++) {
        index = chrominoWork.xIndex + offset.x * iSquare + (chrominoWork.yIndex + offset.y * iSquare) * GameAreaColumnsNumber;
        squareName = "Square_" + index;
        squaresName.push(squareName);
        squareSelector = "#" + squareName;
        switch (iSquare) {
            case 0:
                classColor = chrominoWork.flip ? chrominoWork.colors[2] : chrominoWork.colors[0];
                classOpenSides = chrominoWork.orientation == Orientation.horizontal ? "Square OpenRight" : "Square OpenBottom";
                break;
            case 1:
                classColor = chrominoWork.colors[1];
                classOpenSides = chrominoWork.orientation == Orientation.horizontal ? "Square OpenRightLeft" : "Square OpenBottomTop";
                break;
            case 2:
                classColor = chrominoWork.flip ? chrominoWork.colors[0] : chrominoWork.colors[2];
                classOpenSides = chrominoWork.orientation == Orientation.horizontal ? "Square OpenLeft" : "Square OpenTop";
                break;
        }
        $(squareSelector).removeClass().addClass(classOpenSides + " " + classColor);
    }
    AddHistoryPlay(infoPlayerPlay, squaresName);
    ShowLastChrominoPlayed();
    ResizeGameArea();
}

function AddHistoryPlay(infoPlayerPlay, squaresName) {
    HistoryChrominos.splice(0, 0, { infoPlayerPlay: infoPlayerPlay, square0: squaresName[0], square1: squaresName[1], square2: squaresName[2] });
}

function AddHistorySkipTurn(infoPlayerPlay) {
    HistoryChrominos.splice(0, 0, { infoPlayerPlay: infoPlayerPlay });
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
        $("div[id^='Line_']").children(".Square").each(function (i) {
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
    UpdateSquares(add, GameAreaColumnsNumber, 'top');
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
    UpdateSquares(add, oldColumnsNumber, 'left');
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
    UpdateSquares(add, oldColumnsNumber, 'right');
}

function RemoveChrominoInHand(chrominoId) {
    $('#' + chrominoId).remove();
}
function RefreshButtonNextGame() {
    if (!OpponentsAllBots && !PlayerTurn.isBot && PlayerId != PlayerTurn.id)
        $('#ButtonNextGame').show();
    else
        $('#ButtonNextGame').hide();
}

function RefreshButtonsDrawSkip() {
    if (IsGameFinish) {
        $('#ButtonDrawChromino').hide();
        $('#ButtonSkipTurn').hide();
    }
    else if (PlayerTurn.id != PlayerId) {
        $('#ButtonDrawChromino').hide();
        $('#ButtonSkipTurn').hide();
    }
    else if (InStack > 0 && (!HaveDrew || PlayersNumber == 1)) {
        $('#ButtonDrawChromino').show();
        $('#ButtonSkipTurn').hide();
    }
    else {
        $('#ButtonDrawChromino').hide();
        $('#ButtonSkipTurn').show();
    }
}

function ShowButtonPlayChromino() {
    $('#ButtonPlayingChromino').show();
}
function HideButtonPlayChromino() {
    $('#ButtonPlayingChromino').hide();
}
function ShowButtonChat() {
    if (!OpponentsAllBots)
        $('#ButtonChat').show();
}
function HideButtonChat() {
    $('#ButtonChat').hide();
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

function RefreshHelp() {
    $('.Square').removeClass('Possible');
    if (HelpIndexes.length > 0) {
        let squares = HelpIndexes.map(i => '#Square_' + i).join(", ");
        $(squares).addClass('Possible');
        HelpIndexes = new Array;
    }
    RefreshButtonHelp();
}
function ShowHistoryLatestMove() {
    AnimateChromino(3, true, true);
}

function ShowWorkInProgress() {
    $(':root').css('cursor', 'wait');
    $('#Waiting').show();
}

function ShowWorkIsFinish() {
    $(':root').css('cursor', '');
    $('#Waiting').hide();
}

function UpdateSquares(addNumber, columnsNumber, placeToAdd) {
    HistoryChrominos.forEach(function (item) {
        if (item.square0 !== undefined) {
            for (i = 0; i < 3; i++) {
                squareProp = "square" + i;
                oldNumber = parseInt(item[squareProp].replace("Square_", ""));
                switch (placeToAdd) {
                    case 'top':
                        item[squareProp] = "Square_" + (oldNumber + addNumber * columnsNumber);
                        break;
                    case 'right':
                        item[squareProp] = "Square_" + (oldNumber + addNumber * Math.floor(oldNumber / columnsNumber));
                        break;
                    case 'left':
                        item[squareProp] = "Square_" + (oldNumber + addNumber * (1 + Math.floor(oldNumber / columnsNumber)));
                        break;
                }
            }
        }
    });
};

function RefreshColorsPlayers() {
    Players.forEach(player => RefreshColorPlayer(player));
}

function RefreshColorPlayer(player) {
    if (!player.isBot && player.id != PlayerId) {
        if (player.ongame)
            $(`[player-id='${player.id}']`).removeClass("penpal-status-offline penpal-status-online").addClass("penpal-status-ongame");
        else if (player.online)
            $(`[player-id='${player.id}']`).removeClass("penpal-status-offline penpal-status-ongame").addClass("penpal-status-online");
        else
            $(`[player-id='${player.id}']`).removeClass("penpal-status-online penpal-status-ongame").addClass("penpal-status-offline");
    }
}

function RefreshPlayersStatusIndicator() {
    Players.forEach(p => RefreshPlayerStatus(p));
}

function RefreshPlayerStatus(player) {
    if (player.id != PlayerId && !player.isBot) {
        if (player.ongame) {
            $("#PlayerStatus_" + player.id).removeClass("player-status-offline player-status-online").addClass("player-status-ongame");
            $("#InfoPlayerStatus_" + player.id).text(player.name + " est connecté sur la partie");
        }
        else if (player.online) {
            $("#PlayerStatus_" + player.id).removeClass("player-status-offline player-status-ongame").addClass("player-status-online");
            $("#InfoPlayerStatus_" + player.id).text(player.name + " est connecté sur le site");
        }
        else {
            $("#PlayerStatus_" + player.id).removeClass("player-status-ongame player-status-online").addClass("player-status player-status-offline");
            $("#InfoPlayerStatus_" + player.id).text(player.name + " n'est pas connecté");
        }
    }
    else if (player.id == PlayerId) {
        $("#PlayerStatus_" + player.id).removeClass("player-status-thisplayer").addClass("player-status-thisplayer");
        $("#InfoPlayerStatus_" + player.id).text("Vous");
    }
    else {
        $("#PlayerStatus_" + player.id).removeClass("player-status-bot").addClass("player-status-bot");
        $("#InfoPlayerStatus_" + player.id).text(player.name + " (toujours en ligne)");
    }
}

function MakePlayersStatusIndicators() {
    Players.forEach(MakePlayerStatusIndicator);
}

function MakePlayerStatusIndicator(player, index) {
    let toAdd = `<span id="PlayerStatus_${player.id}" tip="PrivateMessage" class="player-status player-status-${index}"><span id="InfoPlayerStatus_${player.id}" class="info-player-status"></span></span>`;
    $(toAdd).appendTo('#ButtonInfo');
}

function RefreshGameColors() {
    if (PlayerTurn.id != PlayerId) {
        const blueWait = '#07A7D0';
        const greenWait = '#298626';
        const purpleWait = '#380B49';
        const redWait = '#A7000B';
        const yellowWait = '#F4E136';
        const freeWait = '#E0E1E2';
        document.documentElement.style.setProperty('--blue', blueWait);
        document.documentElement.style.setProperty('--green', greenWait);
        document.documentElement.style.setProperty('--purple', purpleWait);
        document.documentElement.style.setProperty('--red', redWait);
        document.documentElement.style.setProperty('--yellow', yellowWait);
        document.documentElement.style.setProperty('--free', freeWait);
    }
    else {
        const blue = '#3AC2EE';
        const green = '#4CAE44';
        const purple = '#571B6D';
        const red = '#FA2C2E';
        const yellow = '#FFEB47';
        const free = '#FFFFFF';
        document.documentElement.style.setProperty('--blue', blue);
        document.documentElement.style.setProperty('--green', green);
        document.documentElement.style.setProperty('--purple', purple);
        document.documentElement.style.setProperty('--red', red);
        document.documentElement.style.setProperty('--yellow', yellow);
        document.documentElement.style.setProperty('--free', free);
    }
}

function WaikUpPlayer() {
    // todo
}

function MakeGameArea(squares, colors) {
    let noneColor = colors.find(x => x.name == "None");
    let cameleonColor = colors.find(x => x.name == "Cameleon");
    let gameAreaDiv = "";
    for (let j = 0; j < GameAreaLinesNumber; j++) {
        gameAreaDiv += `<div id="Line_${j}" class="gameLineArea">`;
        for (let i = 0; i < GameAreaColumnsNumber; i++) {
            let index = i + j * GameAreaColumnsNumber;
            let square = squares[index];
            let colorId = square.color;
            if (colorId != noneColor.id) {
                let classOpenSides = "Square Open";
                if (square.openRight) classOpenSides += "Right";
                if (square.openBottom) classOpenSides += "Bottom";
                if (square.openLeft) classOpenSides += "Left";
                if (square.openTop) classOpenSides += "Top";
                let classColor = colors.find(x => x.id == colorId).name;
                let tip = colorId == cameleonColor ? "tip=Camelon" : "";
                gameAreaDiv += `<div id="Square_${index}" ${tip} class="${classOpenSides} ${classColor}"></div>`;
            }
            else {
                let classColor = "Free";
                let classCloseEdge = "Square";
                if (i < GameAreaColumnsNumber - 1 && squares[i + 1 + j * GameAreaColumnsNumber].color != noneColor.id) classCloseEdge += " CloseRight";
                if (j < GameAreaLinesNumber - 1 && squares[i + (j + 1) * GameAreaColumnsNumber].color != noneColor.id) classCloseEdge += " CloseBottom";
                if (i > 0 && squares[i - 1 + j * GameAreaColumnsNumber].color != noneColor.id) classCloseEdge += " CloseLeft";
                if (j > 0 && squares[i + (j - 1) * GameAreaColumnsNumber].color != noneColor.id) classCloseEdge += " CloseTop";
                gameAreaDiv += `<div id="Square_${index}" class="${classCloseEdge} ${classColor}"></div>`;
            }
        }
        gameAreaDiv += '</div >';
    }
    $(gameAreaDiv).appendTo('#GameArea');
}

function MakeHand(playerChrominosVM, colors) {
    let cameleonColor = colors.find(x => x.name == "Cameleon");
    let handDiv = "";
    for (const chrominoVM of playerChrominosVM) {
        handDiv += `<div id="${chrominoVM.chrominoId}" class="handPlayerChromino">`;
        for (let i = 0; i < 3; i++) {
            let colorId = chrominoVM.squares[i].color;
            let tip = colorId == cameleonColor.id ? "tip=Camelon" : "";
            let classColor = colors.find(x => x.id == colorId).name;
            handDiv += `<div ${tip} class="Square ${classColor}"></div>`;
        }
        handDiv += '</div>';
    }
    if (handDiv != "")
        $(handDiv).appendTo('#Hand');
}