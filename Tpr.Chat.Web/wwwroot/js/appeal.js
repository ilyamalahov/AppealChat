$(document).ready(() => {
    getAccessToken(appealId).then((accessToken) => {
        // Update intreval
        const interval = 10000;

        // Update info hub connection
        const infoConnection = new signalR.HubConnectionBuilder()
            .withUrl("/info")
            .build();

        // Chat hub connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat", { accessTokenFactory: () => accessToken })
            .build();

        // Send message
        const sendMessage = (message) => {
            chatConnection.send('SendMessage', message);

            // 
            $('#messageText').val('').trigger('input');
        };

        // 
        const setExpert = (onlineExpertKey) => {
            const isOnline = onlineExpertKey !== null;

            // Set expert text
            const expertText = isOnline ? '№' + onlineExpertKey : 'отсутствует';

            $('#expertNumber').text(expertText);

            // Change online circle status
            $('#onlineStatus').toggleClass('online', isOnline);
        };

        // Change expert functionality
        const changeExpert = (appeal) => {
            $.ajax({
                method: "post",
                url: "expert/change",
                data: { appealId: appeal },
                beforeSend: () => chatConnection.send("BlockChat", true),
                success: () => $('#modal').showModal("modal/changewait"),
                error: () => chatConnection.send("BlockChat", false)
            });
        };

        // 
        const updateInfo = () => infoConnection.send("MainUpdate", appealId);

        // Receive information response event
        infoConnection.on("ReceiveInfo", (currentDate, remainingTime, isAlarm, isFinished) => {
            // Redirect on consultation end
            if (isFinished) location.reload();

            // Remaining duration
            var remainingDuration = luxon.Duration.fromMillis(remainingTime);

            // Remaining time text
            var remainingText = remainingDuration.toFormat('hh:mm:ss');

            // Remaining format minutes
            var remainingMinutes = remainingDuration.as("minutes");

            if (remainingMinutes < 1) {
                remainingText = 'меньше';
            }

            $('.remaining-value').text(remainingText);

            // Moscow date
            var moscowDate = luxon.DateTime.fromISO(currentDate, { zone: 'utc+3' });

            $('#moscowTime').text(moscowDate.toFormat('t'));

            // Alarm after minutes (5 minutes default)
            if (isAlarm) {
                var alarmText = 'До окончания консультации осталось ' + remainingText + ' минут(-ы)';

                $('#alarm').text(alarmText).show();
            }

            setTimeout(updateInfo, interval);
        });

        // Receiving message from user
        chatConnection.on("Receive", (message) => {
            const isSender = message.nickName === 'Апеллянт';

            const li = receiveMessage(message, isSender);

            $('#messagesList').append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, isAppealOnline, onlineExpertKey) => {
            setExpert(onlineExpertKey);

            const isSender = message.nickName === 'Апеллянт';

            if (!isSender) return;

            const li = joinMessage(message, isSender);

            $("#messagesList").append(li).scrollTo(li);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, onlineExpertKey) => {
            setExpert(onlineExpertKey);

            const isSender = message.nickName === 'Апеллянт';

            if (!isSender) return;

            const li = leaveMessage(message, isSender);

            $("#messagesList").append(li).scrollTo(li);
        });

        // Leave user from chat
        chatConnection.on("FirstJoinExpert", (nickname) => {
            const isSender = nickname === 'Апеллянт';

            const li = firstJoinExpertMessage(nickname, isSender);

            $("#messagesList").append(li).scrollTo(li);
        });

        // 
        chatConnection.on("ToggleChat", (isBlocked) => {
            $('#changeExpertButton').prop('disabled', isBlocked);

            $('#messageForm').toggle(!isBlocked);

            //if (isBlocked) { chatConnection.stop(); }
            //else { chatConnection.start(); }
        });

        // Send message
        $('#sendButton').on('click', () => sendMessage($('#messageText').val()));

        // Textarea auto rows count
        $('#messageText').on('input', function (e) {
            $(this).expandRows();

            // 
            const isDisabled = $(this).val().length === 0;

            $('#sendButton').prop('disabled', isDisabled);
        });

        // Message textarea keyup event
        $('#messageText').on('keyup', function (e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();

                if ($(this).val().length > 0) { sendMessage($(this).val()); }
            }
        });

        // 
        $('#changeExpertButton').on('click', () => changeExpert(appealId));

        // 
        $('#completeButton').on('click', () => $('#modal').showModal('ajax/completechat'));

        // 
        $('#messageText').trigger('input');

        // Start Chat connection
        chatConnection.start()
            .catch((error) => console.error(error.toString()));

        // Start Info connection
        infoConnection.start()
            .then(updateInfo)
            .catch((error) => console.error(error.toString()));
    });
});