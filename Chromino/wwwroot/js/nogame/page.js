$(document).ready(function () {
    AddFeatureNewGame();
    GetGuids();
});

var Guids;

function GetGuids() {
    $.ajax({
        url: '/Games/Guids',
        type: 'GET',
        success: function (data) { CallbackGetGuids(data); },
    });
}

function CallbackGetGuids(data) {
    Guids = data.guids;
    CallSignalR();
}