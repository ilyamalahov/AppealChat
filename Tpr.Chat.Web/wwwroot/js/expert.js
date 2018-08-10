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
        chatConnection.on("Receive", (message, isAppeal) => {
            const messageDate = new Date(message.createDate);

            const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

            const messageInfo = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

            var div = $('<div class="message ' + (isAppeal ? 'place-right' : 'place-left') + '"></div>').html(messageBubble + messageInfo);

            var li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, isAppeal) => {
            const messageDate = new Date(message.createDate);

            const messageElement = '<span class="nickname">' + message.nickName + '</span> подключился к консультации (' + messageDate.toLocaleTimeString() + ')';

            var div = $('<div class="message ' + (isAppeal ? 'place-right' : 'place-left') + '"></div>').html(messageElement);

            var li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);

            // Update status
            if (isAppeal) { changeStatus(true); }
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, isAppeal) => {
            const messageDate = new Date(message.createDate);

            const messageElement = '<span class="nickname">' + message.nickName + '</span> покинул консультацию (' + messageDate.toLocaleTimeString() + ')';

            var div = $('<div class="message ' + (isAppeal ? 'place-right' : 'place-left') + '"></div>').html(messageElement);

            var li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);

            // Update status
            if (isAppeal) { changeStatus(false); }
        });

        $('#sendButton').on('click', (e) => {
            chatConnection.invoke('SendMessage', $('#messageText').val())
                .catch(error => console.error(error))

            $('#messageText').val('');
        });

        const updateCallback = (response) => {
            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);

            $('#remainingTime').text(remainingDuration.toFormat("mm 'минут'"));

            setTimeout(updateInfo, interval, interval, accessToken, updateCallback);
        };

        // Update status
        const changeStatus = function (isOnline) {
            const statusText = isOnline ? 'Подключен к чату' : 'Отключен';

            $('#onlineStatus').text(statusText);
        }

        // Update time information
        const interval = 10000;

        updateInfo(interval, accessToken, updateCallback);

    }).catch(function (error) { alert(error); });
});