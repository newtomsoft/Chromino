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

function RefreshPopupPrivateMessage() {
    if (Penpal.ongame)
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
    RefreshPopupPrivateMessage();
    PrivateMessageGetMessages(onlyNewMessage, true, penpal.id, reset);
    ScrollPrivateMessage();
}

function SwitchToPrivateMessage() {
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
    ChatGetMessages(true, true);
    ScrollChat();
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
    let toAdd = `<div class="selectPenpal" run="SelectPenpal ${player.id}">${player.name}</div>`;
    $(toAdd).appendTo('#PenpalsList');
}

function ScrollChat() {
    console.log("ScrollChat");
    $('#ChatPopupContent').scrollTop($('#ChatPopupContent')[0].scrollHeight);
}

function ScrollPrivateMessage() {
    console.log("ScrollPrivateMessage");
    $('#PrivateMessagePopupContent').scrollTop($('#PrivateMessagePopupContent')[0].scrollHeight);
}