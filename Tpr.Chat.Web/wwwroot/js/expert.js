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
        chatConnection.on("Receive", (message, sender) => {
            const li = receiveMessage(message, sender === "expert");

            $('#messagesList').append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, sender, isAppealOnline, isExpertOnline) => {
            const li = joinUser(message, sender === "expert", isAppealOnline, isExpertOnline);

            $("#messagesList").append(li).scrollTo(li);

            changeStatus(isAppealOnline);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, sender) => {
            const li = leaveUser(message, sender === "expert");

            $("#messagesList").append(li).scrollTo(li);

            changeStatus(false);
        });

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