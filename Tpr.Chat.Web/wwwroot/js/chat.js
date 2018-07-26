$(document).ready(function () {
    // Initializing SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chat?access_token=" + localStorage.getItem("access_token"))
        .configureLogging(signalR.LogLevel.Trace)
        .build();

    // Starting SignalR connection
    connection.start().catch(err => console.error(err));

    // Receiving message from user
    connection.on("Receive", (message) => {
        const messageDate = new Date(message.createDate);

        const messageBubble = $('<div class="message-bubble"></div>').text(message.messageString);

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
    $('#chatForm').on('submit', event => {
        event.preventDefault();
        
        var message = $('#messageInput').val();

        connection.invoke("SendMessage", message)
            .catch(error => console.error(error));
    });

    // Adding message to list
    //this.appendToList = function (messageString, isExpert) {
    //    const li = $('<li class="message' + (isExpert ? ' place-right' : '') + '"></li>').text(messageString);

    //    $("#messagesList").append(li);
    //}
    $('#messageInput').on('input.autoExpand', function () {
        if (this.rows < 5) {
            var minRows = this.rows | 0;
            this.rows = minRows;
            var rows = Math.ceil((this.scrollHeight - this.clientHeight) / 16);
            this.rows = minRows + rows;
        }
    });
});