$(document).ready(function () {
    $.getJSON('js/emoji-list.json', function (data) {
        for (var k in data) {
            var emojiItem = $('<div></div>').html(data[k].unicode);

            emojiItem.bind("click", function (e) {
                insertAtCursor($('#messageInput'), $(this).text());
            });

            $('#emoji-grid').append(emojiItem);
        }
    });

    // Initializing SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chat?access_token=" + sessionStorage.getItem("access_token"))
        .configureLogging(signalR.LogLevel.Trace)
        .build();

    // Starting SignalR connection
    connection.start().catch(err => console.error(err));

    // Receiving message from user
    connection.on("Receive", (message) => {
        const messageDate = new Date(message.createDate);

        const messageBubble = $('<div class="message-bubble"></div>').html(message.messageString);

        const messageSign = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

        const messageElement = $('<li class="message ' + (false ? 'place-right' : 'place-left') + '"></li>').html(messageSign).prepend(messageBubble);

        $("#messagesList").append(messageElement);

        //  this.appendToList(messageString, false);
    });

    // Joining user to chat
    connection.on("Join", (message) => {
        const messageDate = new Date(message.createDate);

        const messageSign = '<span class="nickname">' + message.nickName + '</span> подключился к консультации (' + messageDate.toLocaleTimeString() + ')';

        const li = $('<li class="message ' + (false ? 'place-right' : 'place-left') + '"></li>').html(messageSign);

        $("#messagesList").append(li);

        // this.appendToList(messageString, false);
    });

    // Leave user from chat
    connection.on("Leave", (message) => {
        const messageDate = new Date(message.createDate);

        const messageSign = $('<span class="nickname">' + message.nickName + '</span> покинул консультацию (' + messageDate.toLocaleTimeString() + ')');

        const li = $('<li class="message ' + (false ? 'place-right' : 'place-left') + '"></li>').text(messageSign);

        $("#messagesList").append(li);

        // this.appendToList(messageString, false);
    });

    // Sending message
    $('#sendButton').on('click', event => {
        var message = $('#messageInput').val();
        console.log(message);

        //connection.invoke("SendMessage", message)
        //    .catch(error => console.error(error));
    });

    // Adding message to list
    //this.appendToList = function (messageString, isExpert) {
    //    const li = $('<li class="message' + (isExpert ? ' place-right' : '') + '"></li>').text(messageString);

    //    $("#messagesList").append(li);
    //}
    $('#messageInput').on('input.autoExpand', function () {
        this.rows = $(this).data('min-rows') | 0;

        var currentRowCount = Math.ceil((this.scrollHeight - this.clientHeight) / 16);

        var maxCount = Math.max(0, Math.min(currentRowCount, 5));

        this.rows += maxCount;
    });

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
});