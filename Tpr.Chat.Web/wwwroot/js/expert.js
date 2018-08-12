$(document).ready(function () {
    getAccessToken().then(function (accessToken) {
        // Initializing SignalR connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat?token=" + accessToken)
            .configureLogging(signalR.LogLevel.Trace)
            .build();

        // Starting SignalR connection
        chatConnection.start().catch(error => console.error(error));

        // Receiving message from user
        chatConnection.on("Receive", (message, isAppealSender) => receiveMessage(message, !isAppealSender));

        // Joining user to chat
        chatConnection.on("Join", (message, isAppealSender, isAppealOnline, isExpertOnline) => joinUser(message, !isAppealSender, isAppealOnline, isExpertOnline)); // 

        // Leave user from chat
        chatConnection.on("Leave", (message, isAppealSender) => leaveUser(message, !isAppealSender));

        $('#sendButton').on('click', (e) => {
            chatConnection.invoke('SendMessage', $('#messageText').val())
                .catch(error => console.error(error));

            $('#messageText').val('');
        });

        // Update status
        const changeStatus = function (isOnline) {
            const statusText = isOnline ? 'Подключен к чату' : 'Отключен от чата';

            $('#onlineStatus').text(statusText);
        };

        const updateCallback = function (response) {
            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);

            $('#remainingTime').text(remainingDuration.toFormat("mm 'минут'"));

            setTimeout(updateInfo, interval, interval, accessToken, updateCallback);
        };

        // Update time information
        const interval = 10000;

        updateInfo(interval, accessToken, updateCallback);

    }).catch((error) => alert(error));
});