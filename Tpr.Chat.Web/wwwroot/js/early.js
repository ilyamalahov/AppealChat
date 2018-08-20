$(document).ready(() => {
    // Update information callback
    const updateCallback = (response) => {
        // Remaining duration
        var remainingDuration = luxon.Duration.fromMillis(response.beginTime);

        // Readirect
        var checkRemainingTime = remainingDuration.as('milliseconds');

        if (checkRemainingTime <= 0) { window.location = '/' + appealId; }

        // Remaining shifted object
        var remainingTime = remainingDuration.shiftTo('hours', 'minutes').toObject();

        // Hours
        $('#remainingHours').text(remainingTime.hours);
        // Minutes
        $('#remainingMinutes').text(Math.round(remainingTime.minutes));

        // Recursively update info
        setTimeout(updateInfo, interval, interval, appealId, updateCallback);
    };

    // Update time information
    const interval = 10000;

    updateInfo(interval, appealId, updateCallback);
});