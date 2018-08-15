$(document).ready(function () {
    getAccessToken().then((accessToken) => {

        // Initializing SignalR connection
        const chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chat?token=" + accessToken)
            .configureLogging(signalR.LogLevel.Trace)
            .build();

        // Starting SignalR connection
        chatConnection.start().catch(error => console.error(error));

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

        // Sending message
        $('#sendButton').on('click', () => {
            sendMessage($('#messageText').val());
        });

        // Textarea auto rows count
        $('#messageText').on('input', function (e) {
            var textarea = e.target;

            textarea.rows = calculateExpandRows(textarea);

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

        $('#emojiButton').on('click', () => {
            $('#emojiGrid').toggle();
            $(this).children('i').toggleClass("far fa-smile");
            $(this).children('i').toggleClass("fas fa-chevron-circle-down");
        });

        // 
        $('#switchExpertButton').on('click', () => showModal("ajax/changeexpert"));

        // 
        $('#completeButton').on('click', () => showModal("ajax/complete"));

        $('#completeOkButton').on('click', () => { alert(); closeModal(); });

        const switchLoader = (isVisible) => {
            if (isVisible) {
                $('#changeOverlay').show();
                $('#changeSpinner').show();
            } else {
                $('#changeOverlay').hide();
                $('#changeSpinner').hide();
            }

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

            //textarea.rows = minRows;

            var currentRowCount = Math.ceil((textarea.scrollHeight - textarea.clientHeight) / 16);
            console.log(currentRowCount);

            return Math.max(minRows, Math.min(currentRowCount, 4));
        };

        // Update information callback
        const updateCallback = (response) => {
            var moscowDate = luxon.DateTime.fromMillis(response.moscowDate, { zone: 'utc+3' });

            var remainingDuration = luxon.Duration.fromMillis(response.remainingTime);

            $('#remainingTime').text(remainingDuration.toFormat("mm 'минут'"));

            $('#moscowTime').text(moscowDate.toFormat('t'));

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