$(document).ready(() => {
    // Update time information
    const interval = 10000;

    // Update info hub connection
    const infoConnection = new signalR.HubConnectionBuilder()
        .withUrl("/info")
        .configureLogging(signalR.LogLevel.Trace)
        .build();

    // Receive information response event
    infoConnection.on("ReceiveInfo", (currentDate) => {
        // Moscow date
        var moscowDate = luxon.DateTime.fromISO(currentDate, { zone: 'utc+3' });

        $('#moscowTime').text(moscowDate.toFormat('t'));

        // Timer
        setTimeout(updateInfo, interval);
    });

    // 
    const updateInfo = () => infoConnection.invoke("CompleteUpdate");

    // Start Info connection
    infoConnection.start()
        .then(updateInfo)
        .catch((error) => console.error(error.toString()));
});