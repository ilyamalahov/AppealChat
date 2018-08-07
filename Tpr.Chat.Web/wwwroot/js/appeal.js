
// Chat connection

// Initializing SignalR connection
const chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/chat?token=" + accessToken)
    .configureLogging(signalR.LogLevel.Trace)
    .build();

// Starting SignalR connection
chatConnection.start().catch(error => console.error(error));

// Receiving message from user
chatConnection.on("Receive", (message) => {
    const messageDate = new Date(message.createDate);
    const isAppeal = message.nickName === "Аппелянт";

    const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

    const messageInfo = '<span class="nickname">' + message.nickName + '</span> (' + messageDate.toLocaleTimeString() + ')';

    createListItem(messageBubble + messageInfo, isAppeal);
});

// Joining user to chat
chatConnection.on("Join", (message) => {
    const messageDate = new Date(message.createDate);
    const isAppeal = message.nickName === "Аппелянт";

    const messageElement = '<span class="nickname">' + message.nickName + '</span> подключился к консультации (' + messageDate.toLocaleTimeString() + ')';

    createListItem(messageElement, isAppeal);
});

// Leave user from chat
chatConnection.on("Leave", (message) => {
    const messageDate = new Date(message.createDate);
    const isAppeal = message.nickName === "Аппелянт";

    const messageElement = '<span class="nickname">' + message.nickName + '</span> покинул консультацию (' + messageDate.toLocaleTimeString() + ')';

    createListItem(messageElement, isAppeal);
});

// Sending message
$('#sendButton').on('click', function () { sendMessage($('#messageInput').val()); });

// Textarea auto rows count
$('#messageInput').on('input.autoExpand', function () {
    var maxRows = $(this).data('max-rows');

    this.rows = $(this).data('min-rows') | 0;

    var currentRowCount = Math.ceil((this.scrollHeight - this.clientHeight) / 16);

    var maxCount = Math.max(0, Math.min(currentRowCount, maxRows));

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

// 
const createListItem = function (htmlElement, isAppeal) {
    var div = $('<div class="message ' + (isAppeal ? 'place-left' : 'place-right') + '"></div>').html(htmlElement);

    var li = $('<li></li>').html(div);

    $("#messagesList").append(li).scrollTo(li);
};

const sendMessage = function (message) {
    chatConnection.sendMessage(message)
        .catch(error => console.error(error))
        .finally($('#messageInput').val(''));
};

const setButtonDisable = function (isDisabled) {
    $('#sendButton').prop('disabled', isDisabled);
};

setButtonDisable(true);