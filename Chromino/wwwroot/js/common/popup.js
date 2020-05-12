function ShowPopup(popup, hasCloseButton, argsTab) {
    StopScheduleValidateChromino();
    $(popup).show();
    $(popup).popup({ closebutton: hasCloseButton, autoopen: true, transition: 'all 0.4s' });
    $.fn.popup.defaults.pagecontainer = '#page';
    if (popup == '#PopupChat') {
        ChatGetMessages(true, true);
        $('#ChatPopupContent').scrollTop(300);
        let timeoutScroll;
        clearTimeout(timeoutScroll);
        timeoutScroll = setTimeout(function () {
            $('#ChatPopupContent').scrollTop($('#ChatPopupContent')[0].scrollHeight);
        }, 50);
    }
    else if (popup == '#PopupPrivateMessage') {
        let penpal = Players.find(p => p.id == argsTab[0]);
        let onlyNewMessage = Penpal === penpal ? true : false;
        Penpal = penpal;
        $('#PrivateMessageAdd').attr("recipientId", penpal.id);
        $('#PopupPrivateMessage').attr("penpalid", penpal.id);
        $('#PrivateMessagePenpalName').text(penpal.name);
        RefreshPopupPrivateMessage();
        PrivateMessageGetMessages(onlyNewMessage, true, penpal.id);
        $('#PrivateMessagePopupContent').scrollTop(300);
        let timeoutScroll;
        clearTimeout(timeoutScroll);
        timeoutScroll = setTimeout(function () {
            $('#PrivateMessagePopupContent').scrollTop($('#PrivateMessagePopupContent')[0].scrollHeight);
        }, 50);
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