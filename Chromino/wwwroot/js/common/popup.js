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
        let penpalId = argsTab[0];
        let onlyNewMessage = PenpalId === penpalId ? true : false;
        PenpalId = penpalId;
        $('#PrivateMessageAdd').attr("recipientId", penpalId);
        $('#PopupPrivateMessage').attr("penpalid", penpalId);
        PrivateMessageGetMessages(onlyNewMessage, true, penpalId);
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