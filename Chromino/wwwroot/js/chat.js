var ConnectionHubChat = new signalR.HubConnectionBuilder().withUrl("/hubChat").build();

//Disable send button until connection is established
$("#ButtonChat").hide();

ConnectionHubChat.on("ReceiveMessage", function () {
    if ($("#PopupChat").is(":visible"))
        ChatReadMessages();
    else
        ChatGetMessages(true);
});

ConnectionHubChat.start().then(function () {
    $("#ButtonChat").show();
}).catch(function (err) {
    return console.error(err.toString());
});

function NotifyChat() {
    ConnectionHubChat.invoke("SendMessage").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};