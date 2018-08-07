//$(document).ready(function () {
    getAccessToken().then(function (accessToken) {
        // Initializing SignalR connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat?token=" + accessToken)
            .configureLogging(signalR.LogLevel.Trace)
            .build();

        // Starting SignalR connection
        chatConnection.start().catch(error => console.error(error));

        // Receiving message from user
        chatConnection.on("Receive", (message) => {
            const messageDate = new Date(message.createDate);
            const isAppeal = message.nickName === "Аппелянт";

            const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

            const messageInfo = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

            var div = $('<div class="message ' + (isAppeal ? 'place-left' : 'place-right') + '"></div>').html(messageBubble + messageInfo);

            var li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);
        });


        // Send message
        const sendMessage = function (message) {
            chatConnection.sendMessage(message)
                .catch(error => console.error(error))

            $('#messageText').val('').trigger('change');
        };

        updateInfo(10000, accessToken).then(function (response) {
            console.log(response);
        })
    }).catch(function (error) { alert(error); });
//});