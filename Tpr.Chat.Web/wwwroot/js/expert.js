$(document).ready(function () {
    getAccessToken().then(function (accessToken) {
        // Update info hub connection
        const infoConnection = new signalR.HubConnectionBuilder()
            .withUrl("/info")
            .build();

        // Receive information response event
        infoConnection.on("ReceiveInfo", (currentDate, remainingTime, isAlarm, isFinished) => {
            // Finish consultation
            if (isFinished) completeConsultation();

            // Luxon remaining duration
            var remainingDuration = luxon.Duration.fromMillis(remainingTime);

            // Remaining minutes
            var remainingMinutes = remainingDuration.as('minutes');

            if (remainingMinutes < 1) {
                var remainingText = 'меньше минуты';
            } else {
                var remainingText = remainingDuration.toFormat('m минут(-ы)');
            }

            // 
            $('#remainingTime').text(remainingText);

            // Alarm
            if (isAlarm) {
                var alarmText = 'До окончания консультации осталось ' + remainingText;

                $('#alarm').text(alarmText).show();
            }

            // Timer
            timeoutId = setTimeout(updateInfo, interval);
        });

        var timeoutId;

        // Initializing SignalR connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat", { accessTokenFactory: () => accessToken })
            .build();

        // Receiving message from user
        chatConnection.on("Receive", (message, sender) => {
            const li = receiveMessage(message, sender === "expert");

            $('#messagesList').append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, sender, isAppealOnline, expertKey) => {
            const li = joinMessage(message, sender === "expert");

            $("#messagesList").append(li).scrollTo(li);

            changeStatus(isAppealOnline);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, sender) => {
            const li = leaveMessage(message, sender === "expert");

            $("#messagesList").append(li).scrollTo(li);

            changeStatus(false);
        });

        // 
        $('#sendButton').on('click', () => sendMessage($("#messageText").val()));

        // 
        var isQuickReplyVisible = false;

        // 
        $('#quickReplyButton').on('click', () => {
            isQuickReplyVisible = !isQuickReplyVisible;

            toggleQuickReplyBlock(isQuickReplyVisible);
        });

        // 
        $('.list-item').on('click', function (e) {
            isQuickReplyVisible = false;

            toggleQuickReplyBlock(isQuickReplyVisible);

            $('#messageText').insertAtCursor($(this).text())
                .focus()
                .trigger('input');
        });

        //
        $('#filterText').on('input', function (e) {
            var value = $(this).val();

            $(".list-item").each(function () {
                const isContains = $(this).is(":icontains('" + value + "')");

                $(this).toggle(isContains);
            });
        });

        //
        $('#messageText').on('keyup', function (e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();

                if ($(this).val().length > 0) sendMessage($(this).val());
            }
        });

        //
        $('#messageText').on('input', function (e) {
            const isEnabled = $(this).val().length > 0;

            $('#sendButton').prop('disabled', !isEnabled);
        });

        // 
        $('#messageText').trigger('input');

        // Quick reply block toggle
        const toggleQuickReplyBlock = (isVisibled) => {
            // 
            $('#quickReplyBlock').slideToggle(isVisibled);

            //
            $('#quickReplyButton').toggleClass('send-button', isVisibled);

            // 
            const replyText = isVisibled ? 'Убрать' : 'Быстрый ответ';

            $('#quickReplyButton').text(replyText);

            // 
            $('#filterText').val('').trigger('input');

            // 
            if (isVisibled) { $('#filterText').focus(); }
        };

        // Send message
        const sendMessage = (message) => {
            chatConnection.invoke('SendMessage', message)
                .catch(error => console.error(error.error));

            // 
            $('#messageText').val('').trigger('input');
        };

        // Change online status
        const changeStatus = (isOnline) => {
            const statusText = isOnline ? 'Подключен к чату' : 'Отключен от чата';

            $('#onlineStatus').text(statusText);
        };

        const completeConsultation = () => {
            clearTimeout(timeoutId);

            $('#interactiveBlock').hide();
        };

        // Update time information
        const interval = 10000;

        // 
        const updateInfo = () => infoConnection.invoke("MainUpdate", appealId);

        // Start Chat connection
        chatConnection.start()
            .catch((error) => console.error(error.toString()));

        // Start Info connection
        infoConnection.start()
            .then(updateInfo)
            .catch((error) => console.error(error.toString()));
    }).catch((error) => alert(error.error));
});