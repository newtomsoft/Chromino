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
    if (reset === true || Penpal === undefined)
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
    let penpal = Contacts.find(p => p.id == penpalId);
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
    $('#GameMessageDiv').hide();
    $('#PrivateMessageDiv').show();
    $('#PenpalsList').show();
    $('#PrivateMessagePopupContent').hide();
    $('#ChatTab').removeClass('selected');
    $('#PrivateMessageTab').addClass('selected');
}

function SwitchToChat() {
    $('#PrivateMessageDiv').hide();
    $('#GameMessageDiv').show();
    $('#PrivateMessageTab').removeClass('selected');
    $('#ChatTab').addClass('selected');
    GetChatMessages(true, true);
}

function SelectPenpal(penpalIdTab) {
    let penpalId = parseInt(penpalIdTab[0]);
    SelectPrivateMessageTab(penpalId);
}

function MakePenpalsList() {
    $('.selectPenpal').remove();
    Contacts.forEach(p => MakePenpalList(p));
    UpdateClickRun();
}

function MakePenpalList(player) {
    let spanStatus = `<span player-id='${player.id}' class='penpal-status'></span>`;
    let spanUnreadMessages = `<span class='unread-messages'></span>`;
    let toAdd = `<div id='Penpal_${player.id}' class="selectPenpal"><span run='Chat ${player.id}'> ${player.name}${spanStatus}</span>${spanUnreadMessages}</div>`;
    $(toAdd).appendTo('#PenpalsList');
    RefreshColorPlayer(player);
}

function ScrollChat() {
    if ($('#GameMessagePopupContent').is(":visible"))
        $('#GameMessagePopupContent').scrollTop($('#GameMessagePopupContent')[0].scrollHeight);
}

function ScrollPrivateMessage() {
    if ($('#PrivateMessagePopupContent').is(':visible'))
        $('#PrivateMessagePopupContent').scrollTop($('#PrivateMessagePopupContent')[0].scrollHeight);
}

function OrderPenpalList(penpalId, orderNumber) {
    if (penpalId !== undefined && orderNumber !== undefined)
        $(`#Penpal_${penpalId}`).css("order", `${orderNumber}`);
    else
        for (const contact of Contacts) {
            let index = UnreadPrivatesMessagesNumber.findIndex(x => x.senderId == contact.id);
            if (UnreadPrivatesMessagesNumber[index].number == 0 && !contact.online && !contact.ongame)
                $(`#Penpal_${contact.id}`).css("order", '1');
            else if (UnreadPrivatesMessagesNumber[index].number == 0)
                $(`#Penpal_${contact.id}`).css("order", '0');
        }
}