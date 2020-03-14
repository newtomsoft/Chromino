$(document).ready(function () {
    $(document).click(function () {
        StopDraggable();
        StartDraggable();
    });

    $(document).mouseup(function () {
        MagnetChromino();
    });

    $(window).resize(function () {
        ResizeGameArea();
    });

    ResizeGameArea();
    StartDraggable();

    //animate lasts chrominos played
    if (!PreviouslyDraw && ThisPlayerTurn) {
        AnimateChrominosPlayed(0);
    }
    $("#previousButton").click(function () {
        if (IndexMove < Squares.length / 3 - 1) {
            IndexMove++;
        }
        AnimateChrominosPlayed();
    });
    $("#nextButton").click(function () {
        if (IndexMove > 0) {
            IndexMove--;
        }
        AnimateChrominosPlayed();
    });

    // désactivation du menu contextuel
    window.oncontextmenu = function (event) {
        event.preventDefault();
        event.stopPropagation();
    };
});

//***************************************************//
//**** gestion affichage derniers chrominos joués ***//
//***************************************************//
let IndexMove = 0;
function AnimateChrominosPlayed() {
    index = IndexMove * 3;
    for (let i = index; i < index + 3; i++) {
        for (let iflash = 0; iflash < 3; iflash++) {
            AnimateSquare('#' + Squares[i]);
        }
    }
    $('#PlayerHistoryPseudo').html(Pseudos[IndexMove]).fadeIn().delay(1000).fadeOut();
}
function AnimateSquare(squareId) {
    $(squareId).fadeToggle(150, function () {
        $(this).fadeToggle(150);
    });
}


//***************************************************//
//********** gestion popups de la partie ************//
//***************************************************//
function ShowErrorPopup() {
    $('#errorPopup').show();
    $('#errorPopup').popup({
        autoopen: true,
        transition: 'all 0.4s'
    });
    $.fn.popup.defaults.pagecontainer = '#page'
}

function ShowInfoPopup() {
    $('#infoPopup').show();
    $('#infoPopup').popup({
        autoopen: true,
        transition: 'all 0.4s'
    });
    $.fn.popup.defaults.pagecontainer = '#page'
}
function HideInfoPopup() {
    $('#infoPopup').hide();
}


//***************************************************//
//** gestion déplacements / rotation des chrominos **//
//***************************************************//
let ToRotate = true;
let ToPut = false;
let TimeoutPut = null;
let TimeoutRotate = null;
let OffsetHand = null;
let OffsetGameArea = null;
let LastChrominoMove = null;
let OffsetLastChromino;
let LastChrominoMoveBeforeMove = null;
let OffsetLastChrominoBeforeMove;
let Landscape = true;

function ScheduleRotate() {
    ToRotate = true;
    clearTimeout(TimeoutRotate);
    TimeoutRotate = setTimeout(function () {
        ToRotate = false;
    }, 120);
}

function SchedulePut() {
    clearTimeout(TimeoutPut);
    TimeoutPut = setTimeout(function () {
        let offset = $(LastChrominoMove).offset();
        if (IsChrominoInGameArea() == true && OffsetLastChrominoBeforeMove.left == offset.left && OffsetLastChrominoBeforeMove.top == offset.top) {
            OkToPut();
        }
    }, 600);
}

function OkToPut() {
    ToPut = true;
    for (let iflash = 0; iflash < 2; iflash++) {
        $('#gameArea').fadeToggle(25, function () {
            $(this).fadeToggle(25);
        });
        $(LastChrominoMove).fadeToggle(25, function () {
            $(this).fadeToggle(25);
        });
    }
}

function StartDraggable() {
    $(".handPlayerChromino").draggableTouch()
        .on("dragstart", function () {
            $(this).css('cursor', 'grabbing');
            ScheduleRotate();
            LastChrominoMove = this;
            if ($(LastChrominoMove).css('position') != "fixed") {
                OffsetLastChrominoBeforeMove = $(LastChrominoMove).offset();
                $(LastChrominoMove).css('position', 'fixed');
                $(LastChrominoMove).offset(OffsetLastChrominoBeforeMove);
                ShowTwin(this);
                //$("#" + $(this).attr('id') + "-twin").css('position', 'static');
            }
            OffsetLastChrominoBeforeMove = $(LastChrominoMove).offset();
            SchedulePut();
        }).on("dragend", function () {
            $(this).css('cursor', 'grab');
            LastChrominoMove = this;
            OffsetLastChromino = $(LastChrominoMove).offset();
            MagnetChromino();
            if (!IsChrominoInHand()) {
                HideTwin(this);
                //$("#" + $(this).attr('id') + "-twin").css('position', 'static');
            }
            else if (IsFixed(LastChrominoMove)) {
                let orderFound = 0;
                let orderMax = 0;
                $(".handPlayerChromino").each(function () {
                    if (IsFixed(this) || IsTwin(this, LastChrominoMove))
                        return true;
                    let currentOrder = GetOrder(this);
                    if (currentOrder > orderMax)
                        orderMax = currentOrder;
                    //cas paysage
                    if (Landscape && $(LastChrominoMove).offset().top < $(this).offset().top && orderFound == 0) {
                        orderFound = currentOrder;
                        SetOrder(LastChrominoMove, orderFound);
                        SetTwinOrder(LastChrominoMove, orderFound);
                        SetOrder(this, currentOrder + 1);
                        //SetTwinOrder(this, currentOrder + 1);
                        return true;
                    }
                    if (orderFound != 0 && orderFound <= currentOrder) {
                        SetOrder(this, currentOrder + 1);
                        //SetTwinOrder(this, currentOrder + 1);
                    }
                });
                if (orderFound == 0) {
                    let currentOrder = orderMax + 1;
                    SetOrder(LastChrominoMove, currentOrder);
                    SetTwinOrder(LastChrominoMove, currentOrder);
                }
                $(LastChrominoMove).css('position', 'static');
            }
            if (ToRotate) {
                ToRotate = false;
                clearTimeout(TimeoutRotate);
                Rotation(LastChrominoMove);
                ToPut = false;
            }
            let offset = $(LastChrominoMove).offset();
            if (ToPut && OffsetLastChromino.left == offset.left && OffsetLastChromino.top == offset.top) {
                clearTimeout(TimeoutPut);
                PutChromino();
            }
            ToPut = false;
        });
}

function ShowTwin(chromino) {
    $("#" + $(chromino).attr('id') + "-twin").show();
}

function HideTwin(chromino) {
    $("#" + $(chromino).attr('id') + "-twin").hide();
}

function IsTwin(toTest, chromino) {
    if ($(toTest).attr('id') == $(chromino).attr('id') + "-twin")
        return true;
    else
        return false;
}

function IsFixed(chromino) {
    if ($(chromino).css('position') == "fixed")
        return true;
    else
        return false;
}

function GetOrder(chromino) {
    return parseInt($(chromino).css('order'));
}

function SetOrder(chromino, order) {
    $(chromino).css('order', order);
}

function SetTwinOrder(chromino, order) {
    $("#" + $(chromino).attr('id') + "-twin").css('order', order);
}

function Rotation(chromino) {
    switch (GetRotation(chromino)) {
        case 0:
            SetRotation(chromino, 90);
            break;
        case 90:
            SetRotation(chromino, 180);
            break;
        case 180:
            SetRotation(chromino, 270);
            break;
        case 270:
        default:
            SetRotation(chromino, 0);
            break;
    }
}

function GetRotation(chromino) {
    switch ($(chromino).css("transform")) {
        case "none":
        case "matrix(1, 0, 0, 1, 0, 0)":
            return 0
            break;
        case "matrix(0, 1, -1, 0, 0, 0)":
            return 90;
            break;
        case "matrix(-1, 0, 0, -1, 0, 0)":
            return 180;
            break;
        case "matrix(0, -1, 1, 0, 0, 0)":
        default:
            return 270;
            break;
    }
}

function SetRotation(chromino, rotate) {
    switch (rotate) {
        case 0:
            $(chromino).css("transform", "matrix(1, 0, 0, 1, 0, 0)");
            break;
        case 90:
            $(chromino).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
            break;
        case 180:
            $(chromino).css("transform", "matrix(-1, 0, 0, -1, 0, 0)");
            break;
        case 270:
            $(chromino).css("transform", "matrix(0, -1, 1, 0, 0, 0)");
            break;
    }
}

function StopDraggable() {
    $(this).off("mouseup");
    $(this).off("mousedown");
    $(document).off("mousemove");
    $('.handPlayerChromino').draggableTouch("disable");
}

function MagnetChromino() {
    if (LastChrominoMove != null && IsChrominoInGameArea()) {
        let offset = $(LastChrominoMove).offset();
        let x = offset.left - GameAreaOffsetX;
        let y = offset.top - GameAreaOffsetY;
        let difX = SquareSize * Math.round(x / SquareSize) - x;
        let difY = SquareSize * Math.round(y / SquareSize) - y;
        $(LastChrominoMove).css({ "left": "+=" + difX + "px", "top": "+=" + difY + "px" });
    }
}

function IsChrominoInGameArea() {
    let offset = SquareSize * 2;
    let heightGameArea = $('#gameArea').height();
    let widthGameArea = $('#gameArea').width();
    if (OffsetLastChromino.left + offset >= OffsetGameArea.left && OffsetLastChromino.left <= OffsetGameArea.left + widthGameArea && OffsetLastChromino.top + offset >= OffsetGameArea.top && OffsetLastChromino.top <= OffsetGameArea.top + heightGameArea)
        return true;
    else
        return false;
}

function IsChrominoInHand() {
    let heightHand = $('#hand').height();
    let widthHand = $('#hand').width();

    if (heightHand == 0) {
        heightHand = 100;
    }
    if (widthHand == 0) {
        widthHand = 100;
    }

    if (OffsetLastChromino.left >= OffsetHand.left && OffsetLastChromino.left <= OffsetHand.left + widthHand && OffsetLastChromino.top >= OffsetHand.top && OffsetLastChromino.top <= OffsetHand.top + heightHand)
        return true;
    else
        return false;
}

function PutChromino() {
    if (!ThisPlayerTurn) {
        $('#errorMessage').html("Vous devez attendre votre tour avant de jouer !");
        $('#errorMessageEnd').html("Merci");
        ShowErrorPopup();
        return;
    }
    if (LastChrominoMove != null) {
        switch (GetRotation(LastChrominoMove)) {
            case 0:
                $("#FormOrientation").val(Horizontal);
                break;
            case 90:
                $("#FormOrientation").val(VerticalFlip);
                break;
            case 180:
                $("#FormOrientation").val(HorizontalFlip);
                break;
            case 270:
                $("#FormOrientation").val(Vertical);
                break;
            default:
                break;
        }
        let offset = $(LastChrominoMove).offset();
        let xIndex = Math.round((offset.left - GameAreaOffsetX) / SquareSize);
        let yIndex = Math.round((offset.top - GameAreaOffsetY) / SquareSize);
        $("#FormX").val(xIndex + XMin);
        $("#FormY").val(yIndex + YMin);
        $("#FormChrominoId").val(LastChrominoMove.id);
        $("#FormSendMove").submit();
    }
    else {
        $('#errorMessage').html("Vous devez poser un chromino dans le jeu");
        ShowErrorPopup();
    }
}

//***************************************//
//********* fonctions GameArea  *********//
//***************************************//
let GameAreaOffsetX;
let GameAreaOffsetY;
let SquareSize;

function ResizeGameArea() {
    let documentWidth = $(document).width();
    let documentHeight = $(document).height();
    let width = documentWidth;
    let height = documentHeight;
    if (width > height) {
        Landscape = true;
        width -= 160; //-160 : somme de la taille des 2 bandeaux
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").each(function () {
            $(this).css("transform", "matrix(1, 0, 0, 1, 0, 0)");
            $(this).width(SquareSize * 3);
            $(this).height(SquareSize);
        });
    }
    else {
        Landscape = false;
        height -= 160;
        SquareSize = Math.min(Math.trunc(Math.min(height / GameAreaLinesNumber, width / GameAreaColumnsNumber)), 30);
        $(".handPlayerChromino").each(function () {
            $(this).css("transform", "matrix(0, 1, -1, 0, 0, 0)");
            $(this).width(SquareSize);
            $(this).height(SquareSize * 3);
        });
    }
    $('#gameArea').height(SquareSize * GameAreaLinesNumber);
    $('#gameArea').width(SquareSize * GameAreaColumnsNumber);
    $('.gameLineArea').outerHeight("auto");
    $('.Square').outerHeight(SquareSize);
    $('.Square').outerWidth(SquareSize);
    $('.gameLineArea').css('display', 'flex');
    let gameAreaOffset = $('#gameArea').offset();
    GameAreaOffsetX = gameAreaOffset.left;
    GameAreaOffsetY = gameAreaOffset.top;
    OffsetHand = $('#hand').offset();
    OffsetGameArea = $('#gameArea').offset();
}
