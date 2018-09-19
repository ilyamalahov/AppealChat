$(document).ready(() => {
    // Update info hub connection
    const infoConnection = new signalR.HubConnectionBuilder()
        .withUrl("/info")
        .build();

    // Receive information response event
    infoConnection.on("ReceiveInfo", (currentDate, remainingTime, isStarted) => {
        // Redirect on consultation end
        if (isStarted) location.reload();

        // Moscow date
        var moscowDate = luxon.DateTime.fromISO(currentDate, { zone: 'utc+3' });

        $('#moscowTime').text(moscowDate.toFormat('t'));

        // Luxon remaining duration
        var remainingDuration = luxon.Duration.fromMillis(remainingTime);
        
        // Remaining shifted object
        var remainingTime = remainingDuration.shiftTo('days', 'hours', 'minutes', 'seconds').toObject();

        // Days
        $('#remainingDays').text(remainingTime.days);

        // Hours
        $('#remainingHours').text(remainingTime.hours);

        // Minutes
        $('#remainingMinutes').text(remainingTime.minutes);

        // Timer
        setTimeout(updateInfo, interval);
    });

    // Update time information
    const interval = 10000;

    // 
    const updateInfo = () => infoConnection.invoke("EarlyUpdate", appealId);

    // Start Info connection
    infoConnection.start()
        .then(updateInfo)
        .catch((error) => console.error(error.toString()));
});