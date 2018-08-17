$(document).ready(function () {
    // Update information callback
    const updateCallback = (response) => {
        var remainingDuration = luxon.Duration.fromMillis(response.beginTime);

        var remainingTime = remainingDuration.shiftTo('hours', 'minutes').toObject();

        $('#remainingHours').text(remainingTime.hours);
        $('#remainingMinutes').text(Math.round(remainingTime.minutes));

        setTimeout(updateInfo, interval, interval, appealId, updateCallback);
    };

    // Update time information
    const interval = 10000;

    updateInfo(interval, appealId, updateCallback);
});