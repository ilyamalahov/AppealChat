$(document).ready(function () {
    getAccessToken().then((accessToken) => {
        // Update intreval
        const interval = 10000;

        // Update info hub connection
        const infoConnection = new signalR.HubConnectionBuilder()
            .withUrl("/info")
            .build();

        // Receive information response event
        infoConnection.on("ReceiveInfo", (currentDate, remainingTime, isAlarm, isFinished) => {
            // Redirect on consultation end
            if (isFinished) location.reload();

            // Remaining duration
            var remainingDuration = luxon.Duration.fromMillis(remainingTime);

            // Remaining format minutes
            var remainingMinutes = remainingDuration.as("minutes");

            if (remainingMinutes < 1) {
                var remainingText = 'меньше';
            } else {
                var remainingText = remainingDuration.toFormat('hh:mm:ss');
            }

            $('#remainingTime').text(remainingText);

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

        // Chat hub connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat", { accessTokenFactory: () => accessToken })
            .build();

        // Receiving message from user
        chatConnection.on("Receive", (message, sender) => {
            const li = receiveMessage(message, sender === "appeal");

            $('#messagesList').append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, sender, isAppealOnline, expertKey) => {
            const li = joinMessage(message, sender === "appeal");

            $("#messagesList").append(li).scrollTo(li);

            if (sender === 'expert') joinExpert(expertKey);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, sender, expertKey) => {
            const li = leaveMessage(message, sender === "appeal");

            $("#messagesList").append(li).scrollTo(li);

            if (sender === 'expert') leaveExpert(expertKey);
        });

        // Sending message
        $('#sendButton').on('click', () => sendMessage($('#messageText').val()));

        // Textarea auto rows count
        $('#messageText').on('input', function (e) {
            const textarea = e.target;

            const rows = calculateExpandRows(textarea);

            textarea.rows += rows;

            // 
            const isButtonEnabled = $(this).val().length > 0;

            $('#sendButton').prop('disabled', !isButtonEnabled);
        });

        // Message textarea keyup event
        $('#messageText').on('keyup', function (e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();

                if ($(this).val().length > 0) sendMessage($(this).val());
            }
        });

        // 
        $('#emojiButton').on('click', () => $('#emojiGrid').toggle());

        // 
        $('#switchExpertButton').on('click', () => showModal("ajax/changeexpert"));

        // 
        $('#completeButton').on('click', () => showModal("ajax/complete"));

        // 
        const switchLoader = (isVisibled) => {
            $('#changeOverlay').toggle(isVisibled);
            $('#changeSpinner').toggle(isVisibled);

            $('#switchExpertButton').prop('disabled', !isVisibled);
        };

        // Send message
        const sendMessage = (message) => {
            chatConnection.invoke('SendMessage', message)
                .catch(error => console.error(error.error));

            // 
            $('#messageText').val('').trigger('input');
        };

        // Join expert to chat
        const joinExpert = (expertKey) => {
            const addedSpan = $('<span></span>').text(expertKey);

            $('#expertNumbers').append(addedSpan);
        };

        // Leave expert from chat
        const leaveExpert = (expertKey) => {
            $('span[data-key="' + expertKey + '"]').remove();
        };

        // Update online status
        const changeStatus = (isOnline) => {
            const statusText = isOnline ? 'В сети' : 'Не в сети';

            $('#onlineStatus').text(statusText);
        };

        // 
        const updateInfo = () => infoConnection.invoke("MainUpdate", appealId);

        // 
        $('#messageText').trigger('input');

        // Start Chat connection
        chatConnection.start()
            .catch((error) => console.error(error.toString()));

        // Start Info connection
        infoConnection.start()
            .then(updateInfo)
            .catch((error) => console.error(error.toString()));

        // Modal change button click
        //$(this).on('click', '#changeOkButton', (e) => {
        //    changeExpert(accessToken, () => { closeModal(); switchLoader(true); })
        //        .then(response => {
        //            $('#expertNumber').val(response.expertKey);
        //            switchLoader(false);
        //        })
        //        .catch((error) => { alert(error.error); switchLoader(false); });
        //});
    });
});