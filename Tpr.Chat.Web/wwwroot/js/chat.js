$(document).ready(function () {
    initialization = function (accessToken) {
        $.getJSON('js/emoji-list.json', function (data) {
            for (var k in data) {
                var emojiItem = $('<div></div>').html(data[k].unicode);

                emojiItem.bind("click", function (e) {
                    insertAtCursor($('#messageInput'), $(this).text());
                });

                $('#emojiGrid').append(emojiItem);
            }
        });

        // Initializing SignalR connection
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/chat?token=" + accessToken)
            .configureLogging(signalR.LogLevel.Trace)
            .build();

        // Starting SignalR connection
        connection.start().catch(error => console.error(error));

        // Receiving message from user
        connection.on("Receive", (message) => {
            const messageDate = new Date(message.createDate);
            const isAppeal = message.nickName === "Аппелянт";

            const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

            const messageInfo = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

            createListItem(messageBubble + messageInfo, isAppeal);
        });

        // Joining user to chat
        connection.on("Join", (message) => {
            const messageDate = new Date(message.createDate);
            const isAppeal = message.nickName === "Аппелянт";

            const messageElement = '<span class="nickname">' + message.nickName + '</span> подключился к консультации (' + messageDate.toLocaleTimeString() + ')';

            createListItem(messageElement, isAppeal);
        });

        // Leave user from chat
        connection.on("Leave", (message) => {
            const messageDate = new Date(message.createDate);
            const isAppeal = message.nickName === "Аппелянт";

            const messageElement = '<span class="nickname">' + message.nickName + '</span> покинул консультацию (' + messageDate.toLocaleTimeString() + ')';

            createListItem(messageElement, isAppeal);
        });

        // Sending message
        $('#sendButton').on('click', function () { sendMessage($('#messageInput').val()); });

        // Textarea auto rows count
        $('#messageInput').on('input.autoExpand', function () {
            this.rows = $(this).data('min-rows') | 0;

            var currentRowCount = Math.ceil((this.scrollHeight - this.clientHeight) / 16);

            var maxCount = Math.max(0, Math.min(currentRowCount, 5));

            this.rows += maxCount;
        });

        $('#emojiButton').on('click', function () {
            $('#emojiGrid').toggle();
            $(this).children('i').toggleClass("far fa-smile");
            $(this).children('i').toggleClass("fas fa-chevron-circle-down");
        });

        $('#messageInput').on('keypress', function (e) {
            if (e.keyCode === 13 && !e.shiftKey) {
                e.preventDefault();

                if ($(this).val() !== '') {
                    sendMessage($(this).val());
                }
            }
        });

        $('#messageInput').on('change keyup', function (e) {
            setButtonDisable($(this).val() === '');
        });

        $('#showInfoPanelButton').on('click', function () { switchInfoPanel(true); });

        $('#hideInfoPanelButton').on('click', function () { switchInfoPanel(false); });

        // 
        createListItem = function (htmlElement, isAppeal) {
            var div = $('<div class="message ' + (isAppeal ? 'place-left' : 'place-right') + '"></div>').html(htmlElement);

            var li = $('<li></li>').html(div);

            $("#messagesList").append(li).scrollTo(li);
        };

        // Insert text in textarea at cursor position
        insertAtCursor = function (element, value) {
            if (document.selection) {
                element.focus();
                var selection = document.selection.createRange();
                selection.text = value;
            } else if (element.prop('selectionStart') || element.prop('selectionStart') === '0') {
                var startSubstring = element.val().substring(0, element.prop('selectionStart'));
                var endSubstring = element.val().substring(element.prop('selectionEnd'), element.val().length);
                element.val(startSubstring + value + endSubstring);
            } else {
                element.val(element.val() + value);
            }
        };

        sendMessage = function (message) {
            connection.invoke("SendMessage", message)
                .catch(error => console.error(error))
                .finally($('#messageInput').val(''));
        };

        jQuery.fn.scrollTo = function (element) {
            $(this).scrollTop($(this).scrollTop() - $(this).offset().top + $(element).offset().top);

            return this;
        };

        setButtonDisable = function (isDisabled) {
            $('#sendButton').prop('disabled', isDisabled);
        };

        switchInfoPanel = function (isVisible) {
            if (isVisible) {
                $('#infoPanel').show();
                $('#showInfoPanelButton').hide();
            } else {
                $('#infoPanel').hide();
                $('#showInfoPanelButton').show();
            }
        };
        updateInfo = function (interval) {
            $.ajax({
                method: "POST",
                url: "update",
                headers: { "Authorization": "Bearer " + sessionStorage.getItem("access_token") },
                success: function (response) {
                    // Current Time
                    var currentDate = new Date(response.currentDate);
                    $('#moscowTime').text(currentDate.toLocaleTimeString());

                    // Remaining Time
                    var remainingDate = new Date(response.remainingTime);
                    $('#remainingTime').text(remainingDate.toLocaleTimeString());

                    // Recursive invoke setTimeout()
                    setTimeout(updateInfo, interval, interval);
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });
        };

        setButtonDisable(true);

        switchInfoPanel(true);

        updateInfo(10000);
    };

    var accessToken = sessionStorage.getItem('access_token');

    if (accessToken) {
        initialization(accessToken);
    } else {
        $.post({
            method: "POST",
            url: "token",
            data: { appealId: appealId, key: expertKey },
            success: function (response) {
                sessionStorage.setItem("access_token", response.accessToken);

                initialization(response.accessToken);
            },
            error: function (xhr, status, error) {
                window.location.href = "/error";
            },
            async: false
        })
    }
});