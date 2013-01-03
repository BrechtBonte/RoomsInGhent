$(function(){

    // Set Countdowns
    var counts = $('.countdown');

    if (counts.length > 0) {

        calculateCountdown();

        var t = setInterval(calculateCountdown, 1000);
    }

    // Enable enlargement of imgs (room detail)
    var smalls = $("#smallImgs img");

    if ($("#bigImg").length > 0 && smalls.length > 0) {

        $("#bigImg img").attr("src", $(smalls[0]).attr("src"));

        smalls.each(function(i,el) {
            $(el).click(function () {
                $("#bigImg img").attr("src", $(el).attr("src"));
            });
        });
    }

});


// Calculate the countdown
function calculateCountdown() {
    var counts = $('.countdown');

    for (var i = 0; i < counts.length; i++) {
        var span = $(counts[i]);
        var saved = new Date(span.data('since'));
        var now = new Date();
        var difference = (now - saved);
        if (difference <= 0) location.reload();
        difference = Math.round(difference / 1000);
        var sec = 59 - (difference % 60);
        difference = Math.floor(difference / 60);
        var min = 59 - (difference % 60);
        var hour = 23 - (Math.floor(difference / 60));
        span.html(hour + ':' + min + ':' + sec);
    }
}