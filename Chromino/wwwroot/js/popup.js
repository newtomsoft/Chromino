function ShowPopup(popup, hasCloseButton, argsTab) {
    $('.popup').popup('hide');
    StopScheduleValidateChromino();
    $(popup).show();
    $(popup).popup({ closebutton: hasCloseButton, autoopen: true, transition: 'all 0.4s' });
    $.fn.popup.defaults.pagecontainer = '#page';
    if (popup == '#PopupChat') {
        if (argsTab.length == 0) {
            SelectChatTab();
        }
        else {
            SelectPrivateMessageTab(argsTab[0]);
        }
    }
}

function ClosePopup(popup) {
    $(popup).popup('hide');
}

function RefreshPenpalTitleInPopupPrivateMessage(reset) {
    if (reset === true)
        $('#PrivateMessagePenpalStatus').removeClass().addClass("penpal-status");
    else if (Penpal.ongame)
        $('#PrivateMessagePenpalStatus').removeClass().addClass("penpal-status penpal-status-ongame");
    else if (Penpal.online)
        $('#PrivateMessagePenpalStatus').removeClass().addClass("penpal-status penpal-status-online");
    else
        $('#PrivateMessagePenpalStatus').removeClass().addClass("penpal-status penpal-status-offline");
}

function SelectChatTab() {
    SwitchToChat();
}

function SelectPrivateMessageTab(penpalId) {
    SwitchToPrivateMessage();
    $('#PenpalsList').hide();
    $('#PrivateMessagePopupContent').show();
    let penpal = HumansAll.find(p => p.id == penpalId);
    let onlyNewMessage = false;
    let reset = true;
    if (Penpal === penpal) {
        onlyNewMessage = true;
        reset = false;
    }
    Penpal = penpal;
    $('#PrivateMessageAdd').attr("recipientId", penpal.id);
    $('#PopupChat').attr("penpalid", penpal.id);
    $('#PrivateMessagePenpalName').text(penpal.name);
    RefreshPenpalTitleInPopupPrivateMessage();
    GetPrivateMessageMessages(onlyNewMessage, true, penpal.id, reset);
    ScrollPrivateMessage();
}

function SwitchToPrivateMessage() {
    $('#PrivateMessagePenpalName').html("");
    $('#PrivateMessagePenpalStatus').html("");
    RefreshPenpalTitleInPopupPrivateMessage(true);
    RefreshPenpalsList();
    $('#ChatDiv').hide();
    $('#PrivateMessageDiv').show();
    $('#PenpalsList').show();
    $('#PrivateMessagePopupContent').hide();
    $('#ChatTab').removeClass('selected');
    $('#PrivateMessageTab').addClass('selected');
}

function SwitchToChat() {
    $('#PrivateMessageDiv').hide();
    $('#ChatDiv').show();
    $('#PrivateMessageTab').removeClass('selected');
    $('#ChatTab').addClass('selected');
    GetChatMessages(true, true);
}

function SelectPenpal(penpalIdTab) {
    let penpalId = parseInt(penpalIdTab[0]);
    SelectPrivateMessageTab(penpalId);
}

function RefreshPenpalsList() {
    $('.selectPenpal').remove();
    HumansAll.forEach(p => RefreshPenpalList(p));
    UpdateClickRun();
}

function RefreshPenpalList(player) {
    let spanPlayerId = `<span player-id='${player.id}' class='penpal-status'></span>`;
    let toAdd = `<div class="selectPenpal"><span run='Chat ${player.id}'> ${player.name}${spanPlayerId}</span></div>`;
    $(toAdd).appendTo('#PenpalsList');
    RefreshColorPlayer(player);
}

function ScrollChat() {
    if ($('#ChatPopupContent').is(":visible"))
        $('#ChatPopupContent').scrollTop($('#ChatPopupContent')[0].scrollHeight);
}

function ScrollPrivateMessage() {
    if ($('#PrivateMessagePopupContent').is(':visible'))
        $('#PrivateMessagePopupContent').scrollTop($('#PrivateMessagePopupContent')[0].scrollHeight);
}