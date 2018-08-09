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
            //const isAppeal = message.nickName === "Аппелянт";

            const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

            const messageInfo = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

            createListItem(messageBubble + messageInfo, isAppeal);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, isAppeal) => {
            const messageDate = new Date(message.createDate);
            //const isAppeal = message.nickName === "Аппелянт";

            const messageElement = '<span class="nickname">' + message.nickName + '</span> подключился к консультации (' + messageDate.toLocaleTimeString() + ')';

            createListItem(messageElement, isAppeal);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, isAppeal) => {
            const messageDate = new Date(message.createDate);
            //const isAppeal = message.nickName === "Аппелянт";

            const messageElement = '<span class="nickname">' + message.nickName + '</span> покинул консультацию (' + messageDate.toLocaleTimeString() + ')';

            createListItem(messageElement, isAppeal);
        });

        // Sending message
        $('#sendButton').on('click', () => {
            sendMessage($('#messageText').val());
        });

        // Textarea auto rows count
        $('#messageText').on('input.autoExpand', function () {
            //var maxRows = $(this).data('max-rows') | 1;

            this.rows = $(this).data('min-rows') | 0;

            var currentRowCount = Math.ceil((this.scrollHeight - this.clientHeight) / 16);

            var maxCount = Math.max(0, Math.min(currentRowCount, 4));

            this.rows += maxCount;
        });

        $('#messageText').on('change keyup', function (e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();

                if ($(this).val() !== '') {
                    sendMessage($(this).val());
                }
            }

            setButtonDisable($(this).val() === '');
        });

        $('#emojiButton').on('click', function () {
            $('#emojiGrid').toggle();
            $(this).children('i').toggleClass("far fa-smile");
            $(this).children('i').toggleClass("fas fa-chevron-circle-down");
        });

        // 
        const createListItem = function (htmlElement, isAppeal) {
            var div = $('<div class="message ' + (isAppeal ? 'place-left' : 'place-right') + '"></div>').html(htmlElement);

            var li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);
        };

        const sendMessage = function (message) {
            chatConnection.invoke('SendMessage', message)
                .catch(error => console.error(error))

            $('#messageText').val('');
        };

        const setButtonDisable = function (isDisabled) {
            $('#sendButton').prop('disabled', isDisabled);
        };

        // Update information callback
        const updateCallback = (response) => {
            var moscowDate = luxon.DateTime.fromMillis(response.moscowDate, { zone: 'utc+3' });

            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);

            $('#remainingTime').text(remainingDuration.toFormat("mm 'минут'"));

            $('#moscowTime').text(moscowDate.toFormat('hh:mm'));

            setTimeout(updateInfo, interval, interval, accessToken, updateCallback);
        };

        // Disable Send button
        setButtonDisable(true);

        // Update time information
        const interval = 10000;

        updateInfo(interval, accessToken, updateCallback);
    });
});