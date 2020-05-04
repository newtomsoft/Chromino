var ConnectionHubChat = new signalR.HubConnectionBuilder().withUrl("/hubChat").build();

//Disable send button until connection is established
$("#ButtonChat").hide();

ConnectionHubChat.on("ReceiveMessage", function () {
    NotReadMessages++;
    RefreshButtonChat();
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