let Refresh;
let XBegin, YBegin;
let XEnd, YEnd;
let XDiff, YDiff;
let TimeStart;

function TouchStart() {
    if ($(window).scrollTop() < 10)
    {
        Refresh = true;
        TimeStart = new Date().getTime();
        XBegin = event.touches[0].clientX;
        YBegin = event.touches[0].clientY;
        XDiff = 0;
        YDiff = 0;
    }
    else 
        Refresh = false;
}

function TouchMove() {
    XEnd = event.touches[0].clientX;
    YEnd = event.touches[0].clientY;
}

function TouchEnd() {
    if (Refresh) {
        XDiff = XEnd - XBegin;
        YDiff = YEnd - YBegin;
        TimeDiff = new Date().getTime() - TimeStart;
        if (YDiff > 150 && Math.abs(XDiff) < 30 && TimeDiff < 150) {
            $("body").append($('<div class="refresh"></div>'));
            location.reload();
        }
    }
}