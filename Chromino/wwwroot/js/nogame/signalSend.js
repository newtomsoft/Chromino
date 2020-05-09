function SendAddToGroup() {
    for (var i = 0; i < Guids.length; i++) {
        let guid = Guids[i];
        ConnectionHubGame.invoke("AddToGroup", guid).catch(function (err) { return console.error(err.toString()); });
    }
};
