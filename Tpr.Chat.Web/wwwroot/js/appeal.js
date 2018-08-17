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
        $('#sendButton').on('click', () => {
            sendMessage($('#messageText').val());
        });

        // Textarea auto rows count
        $('#messageText').on('input', function (e) {
            var textarea = e.target;

            var rows = calculateExpandRows(textarea);

            textarea.rows += rows;

            setButtonDisable($(this).val() === '');
        });

        $('#messageText').on('keyup', function (e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();

                if ($(this).val() !== '') {
                    sendMessage($(this).val());
                }
            }
        });

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
            var div = $('<div class="message ' + (isAppeal ? 'place-left' : 'place-right') + '"></div>').html(htmlElement);

            var li = $('<li></li>').html(div);

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
            //var maxRows = $(this).data('max-rows') | 1;

            var minRows = textarea.dataset.minRows | 1;

            textarea.rows = minRows;

            var currentRowCount = Math.ceil((textarea.scrollHeight - textarea.clientHeight) / 16);

            return Math.max(0, Math.min(currentRowCount, 4));
        };

        // Update information callback
        const updateCallback = (response) => {
            var moscowDate = luxon.DateTime.fromMillis(response.moscowDate, { zone: 'utc+3' });

            $('#moscowTime').text(moscowDate.toFormat('t'));

            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);

            var remainingMinutes = remainingDuration.toFormat("m");

            $('#remainingTime').text(remainingMinutes);

            if (response.isAlarm) {
                var alarmText = 'До окончания консультации осталось ' + remainingMinutes + ' минут(-ы)';

                $('#alarm').text(alarmText).show();
            }

            setTimeout(updateInfo, interval, interval, accessToken, updateCallback);
        };

        // Update status
        const changeStatus = (isOnline) => {
            const statusText = isOnline ? 'В сети' : 'Не в сети';

            $('#onlineStatus').text(statusText);
        };

        // Update time information
        const interval = 10000;

        updateInfo(interval, accessToken, updateCallback);

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