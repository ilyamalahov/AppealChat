$(document).ready(function () {
    $.getJSON('js/emoji-list.json', function (data) {
        for (var k in data) {
            var emojiItem = $('<div></div>').html(data[k].unicode);

            emojiItem.bind("click", function (e) {
                insertAtCursor($('#messageInput'), $(this).text());
            });

            $('#emojiGrid').append(emojiItem);
        }

        $('#emojiGrid').hide();
    });

    // Initializing SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chat?access_token=" + sessionStorage.getItem("access_token"))
        .configureLogging(signalR.LogLevel.Trace)
        .build();

    // Starting SignalR connection
    connection.start().catch(error => console.error(error));

    // Receiving message from user
    connection.on("Receive", (message) => {
        const messageDate = new Date(message.createDate);
        const isExpert = message.nickName == "Консультант";

        const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

        const messageInfo = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

        createListItem(messageBubble + messageInfo, isExpert);
    });

    // Joining user to chat
    connection.on("Join", (message) => {
        const messageDate = new Date(message.createDate);
        const isExpert = message.nickName == "Консультант";

        const messageElement = '<span class="nickname">' + message.nickName + '</span> подключился к консультации (' + messageDate.toLocaleTimeString() + ')';

        createListItem(messageElement, isExpert);
    });

    // Leave user from chat
    connection.on("Leave", (message) => {
        const messageDate = new Date(message.createDate);
        const isExpert = message.nickName == "Консультант";

        const messageElement = '<span class="nickname">' + message.nickName + '</span> покинул консультацию (' + messageDate.toLocaleTimeString() + ')';

        createListItem(messageElement, isExpert);
    });

    createListItem = function (htmlElement, isExpert) {
        var div = $('<div class="message ' + (isExpert ? 'place-right' : 'place-left') + '"></div>').html(htmlElement);

        var li = $('<li></li>').html(div);

        $("#messagesList").append(li).scrollTo(li);
    }

    // Sending message
    $('#sendButton').on('click', event => {
        var message = $('#messageInput').val();

        connection.invoke("SendMessage", message)
            .catch(error => console.error(error))
            .finally($('#messageInput').val(''));
    });

    // Textarea auto rows count
    $('#messageInput').on('input.autoExpand', function () {
        this.rows = $(this).data('min-rows') | 0;

        var currentRowCount = Math.ceil((this.scrollHeight - this.clientHeight) / 16);

        var maxCount = Math.max(0, Math.min(currentRowCount, 5));

        this.rows += maxCount;
    });

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
    }

    jQuery.fn.scrollTo = function (element) {
        $(this).scrollTop($(this).scrollTop() - $(this).offset().top + $(element).offset().top);

        return this;
    };

    $('#emojiToggle').on('click', function () {
        $('#emojiGrid').toggle();
    });

    $("#messageInput").on("change", function () {
        var enableAttr = $(this).val() ? 'enabled' : 'disabled';

        $("#sendButton").attr(enableAttr, enableAttr);
    });
});