function InitGameArea(squares, colors) {
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

function InitHand(playerChrominosVM, colors) {
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