﻿$(document).ready(function () {
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
        chatConnection.on("Receive", (message) => {
            const isSender = message.nickName === 'Член КК № ' + expertKey;

            const li = receiveMessage(message, isSender);

            $('#messagesList').append(li).scrollTo(li);
        });

        // Joining user to chat
        chatConnection.on("Join", (message, isAppealOnline, onlineExpertKey) => {
            changeStatus(isAppealOnline);

            const isSender = message.nickName === 'Член КК № ' + expertKey;

            const li = joinMessage(message, isSender);

            $("#messagesList").append(li).scrollTo(li);
        });

        // Leave user from chat
        chatConnection.on("Leave", (message) => {
            changeStatus(false);

            const isSender = message.nickName === 'Член КК № ' + expertKey;
            
            const li = leaveMessage(message, isSender);

            $("#messagesList").append(li).scrollTo(li);
        });

        // Leave user from chat
        chatConnection.on("FirstJoinExpert", (nickname) => {
            const isSender = nickname !== 'Аппелянт';

            const li = firstJoinExpertMessage(nickname, isSender);

            $("#messagesList").append(li).scrollTo(li);
        });

        // 
        $('#sendButton').on('click', () => sendMessage($("#messageText").val()));

        // 
        var quickReplyIsVisible = false;

        // 
        $('#quickReplyButton').on('click', () => toggleQuickReplyBlock(!quickReplyIsVisible));

        // 
        $('#replyList').on('click', 'li', function (e) {
            toggleQuickReplyBlock(false);

            $('#messageText').insertAtCursor($(this).text())
                .focus()
                .trigger('input');
        });

        //
        $('#filterText').on('input', function (e) {
            var value = $(this).val();

            $("#replyList li")
                .hide()
                .filter(":icontains('" + value + "')")
                .show()
                .each((index, element) => $(element).highlightText(value));
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
            const isDisabled = $(this).val().length === 0;

            $('#sendButton').prop('disabled', isDisabled);
        });

        // 
        $('#messageText').trigger('input');

        // Quick reply block toggle
        const toggleQuickReplyBlock = (isVisible) => {
            quickReplyIsVisible = isVisible;

            // 
            if (isVisible) {
                $('#quickReplyButton>img').attr('src', 'images/down-chevron.svg');
                $('#quickReplyBlock').slideDown('fast', () => $('#filterText').focus());
                $('#quickReplyOverlay').fadeIn('fast');
            } else {
                $('#quickReplyButton>img').attr('src', 'images/question.svg');
                $('#quickReplyBlock').slideUp('fast', () => $('#filterText').val('').trigger('input'));
                $('#quickReplyOverlay').fadeOut('fast');
            }
        };

        // Send message
        const sendMessage = (message) => {
            chatConnection.invoke('SendMessage', message)
                .catch(error => console.error(error.error));

            // 
            $('#messageText').val('').trigger('input');
        };

        // Change online status
        const changeStatus = (isOnline) => $('#onlineStatus').toggleClass('online', isOnline);

        const completeConsultation = () => {
            chatConnection.stop();
            infoConnection.stop();

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