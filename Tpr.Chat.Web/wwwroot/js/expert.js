$(document).ready(function () {
    getAccessToken().then(function (appealId) {
        // Initializing SignalR connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat?token=" + appealId)
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

        // 
        var isQuickReplyVisible = false;

        $('#quickReplyButton').on('click', () => {
            isQuickReplyVisible = !isQuickReplyVisible;

            toggleQuickReplyBlock(isQuickReplyVisible);
        });

        $('.list-item').on('click', function (e) {
            toggleQuickReplyBlock(false);

            insertAtCursor($('#messageText'), $(this).text());
        });

        $('#filterText').on('input', function (e) {
            var value = $(this).val();

            $(".list-item").each(function () {
                const isContains = $(this).is(":icontains('" + value + "')");

                $(this).toggle(isContains);
            });
        });

        const toggleQuickReplyBlock = (isVisible) => {
            $('#quickReplyBlock').slideToggle(isVisible);
            $('#quickReplyButton').toggleClass('send-button', isVisible);

            // 
            const replyText = isVisible ? 'Убрать' : 'Быстрый ответ';

            $('#quickReplyButton').text(replyText);

            if (isVisible) {
                $('#filterText').focus();
            }
        };

        // Update status
        const changeStatus = function (isOnline) {
            const statusText = isOnline ? 'Подключен к чату' : 'Отключен от чата';

            $('#onlineStatus').text(statusText);
        };

        const updateCallback = function (response) {
            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);

            var remainingMinutes = remainingDuration.toFormat('m');

            $('#remainingTime').text(remainingMinutes);

            if (response.isAlarm) {
                var alarmText = 'До окончания консультации осталось ' + remainingMinutes + ' минут(-ы)';

                $('#alarm').text(alarmText).show();
            }

            setTimeout(updateInfo, interval, interval, appealId, updateCallback);
        };

        // Update time information
        const interval = 10000;

        updateInfo(interval, appealId, updateCallback);

    }).catch((error) => alert(error));
});