$(document).ready(function () {
    getAccessToken().then((accessToken) => {

        //const infoConnection = new signalR.HubConnectionBuilder()
        //    .withUrl("/info")
        //    .configureLogging(signalR.LogLevel.Trace)
        //    .build();

        //// Starting SignalR connection
        //infoConnection.start()
        //    .then(() => infoConnection.send('MainUpdate', appealId))
        //    .catch(console.log("Error connection"));

        //// Receiving message from user
        //infoConnection.on("ReceiveInfo", (currentDate, remainingTime, isAlarm) => {
        //    console.log(currentDate, remainingTime, isAlarm);
        //    var moscowDate = luxon.DateTime.fromISO(currentDate, { zone: 'utc+3' });
            
        //    $('#moscowTime').text(moscowDate.toFormat('t'));

        //    var remainingDuration = luxon.Duration.fromMillis(remainingTime);

        //    var remainingMinutes = remainingDuration.toFormat("m");

        //    $('#remainingTime').text(remainingMinutes);

        //    if (isAlarm) {
        //        var alarmText = 'До окончания консультации осталось ' + remainingMinutes + ' минут(-ы)';

        //        $('#alarm').text(alarmText).show();
        //    }
        //});


        // Initializing SignalR connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat", { accessTokenFactory: () => accessToken })
            //.configureLogging(signalR.LogLevel.Trace)
            .build();

        // Starting SignalR connection
        chatConnection.start();

        // Receiving message from user
        chatConnection.on("Receive", (message, sender) => {
            const li = receiveMessage(message, sender === "appeal");

            $('#messagesList').append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, sender, isAppealOnline, isExpertOnline) => {
            const li = joinUser(message, sender === "appeal", isAppealOnline, isExpertOnline);

            $("#messagesList").append(li).scrollTo(li);

            changeStatus(isExpertOnline);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message, sender) => {
            const li = leaveUser(message, sender === "appeal");

            $("#messagesList").append(li).scrollTo(li);

            changeStatus(false);
        });

        $('#moscowTime').on('click', function (e) {
            infoConnection.invoke('MainUpdate');
        });

        // Sending message
        $('#sendButton').on('click', () => sendMessage($('#messageText').val()));

        // Textarea auto rows count
        $('#messageText').on('input', function (e) {
            const textarea = e.target;

            const rows = calculateExpandRows(textarea);

            textarea.rows += rows;

            setButtonDisable($(this).val() === '');
        });

        $('#messageText').on('keyup', messageTextKeyup);

        // 
        $('#emojiButton').on('click', () => $('#emojiGrid').toggle());

        // 
        $('#switchExpertButton').on('click', () => showModal("ajax/changeexpert"));

        // 
        $('#completeButton').on('click', () => showModal("ajax/complete"));

        $('#completeOkButton').on('click', () => { alert(); closeModal(); });

        const switchLoader = (isVisible) => {
            $('#changeOverlay').toggle(isVisible);
            $('#changeSpinner').toggle(isVisible);

            $('#switchExpertButton').prop('disabled', !isVisible);
        };

        // 
        const createListItem = (htmlElement, isAppeal) => {
            const div = $('<div class="message ' + (isAppeal ? 'place-left' : 'place-right') + '"></div>').html(htmlElement);

            const li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);
        };

        const sendMessage = (message) => {
            chatConnection.invoke('SendMessage', message)
                .catch(error => console.error(error));

            $('#messageText').val('').trigger('input');
        };

        const setButtonDisable = (isDisabled) => {
            $('#sendButton').prop('disabled', isDisabled);
        };

        const calculateExpandRows = (textarea) => {
            const minRows = textarea.dataset.minRows | 1;

            textarea.rows = minRows;

            const currentRowCount = Math.ceil((textarea.scrollHeight - textarea.clientHeight) / 16);

            return Math.max(0, Math.min(currentRowCount, 4));
        };

        // Update information callback
        const updateCallback = (response) => {
            // Remaining duration
            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);
            
            // Redirect
            const checkRemainingTime = remainingDuration.as("milliseconds");

            if (checkRemainingTime <= 0) { window.location = '/' + appealId; }

            // Remaining format minutes
            var remainingMinutes = remainingDuration.as("minutes");

            if (remainingMinutes <= 0) {
                var remainingMinutes = 'меньше минуты';
            } else {
                var remainingMinutes = remainingDuration.toFormat('m минут(-ы)');
            }

            $('#remainingTime').text(remainingMinutes);

            // Moscow date
            var moscowDate = luxon.DateTime.fromISO(response.moscowDate, { zone: 'utc+3' });

            $('#moscowTime').text(moscowDate.toFormat('t'));

            // Alarm after minutes (5 minutes default)
            if (response.isAlarm) {
                var alarmText = 'До окончания консультации осталось ' + remainingMinutes;

                $('#alarm').text(alarmText).show();
            }

            // Recursively update info
            setTimeout(updateInfo, interval, interval, appealId, updateCallback);
        };

        // Update status
        const changeStatus = (isOnline) => {
            const statusText = isOnline ? 'В сети' : 'Не в сети';

            $('#onlineStatus').text(statusText);
        };

        // Update time information
        const interval = 10000;

        // Start update time information
        updateInfo(interval, appealId, updateCallback);

        // Modal change button click
        $(this).on('click', '#changeOkButton', (e) => {
            changeExpert(accessToken, () => { closeModal(); switchLoader(true); })
                .then(response => {
                    $('#expertNumber').val(response.expertKey);
                    switchLoader(false);
                })
                .catch((error) => { alert(error.error); switchLoader(false); });
        });
    });
});