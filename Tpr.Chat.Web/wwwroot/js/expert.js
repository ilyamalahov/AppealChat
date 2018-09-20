// Variables

// Quick reply block is visible?
var quickReplyIsVisible = false;

// Update time interval in milliseconds
const updateInterval = 1000;

// Objects

// Update info hub connection
var infoConnection;

// Chat hub connection
var chatConnection;

// Functions

// Update time info
const updateInfo = () => infoConnection.invoke("MainUpdate", appealId);

// Receive info callback
const onReceiveInfo = (currentDate, remainingTime, isAlarm, isFinished) => {
    // Finish consultation
    if (isFinished) {
        // Stop info hub connection
        infoConnection.stop();

        // Stop chat hub connection
        chatConnection.stop();

        // Hide alarm
        $('#alarm').hide();

        return;
    }

    // Alarm
    if (isAlarm) {
        const alarmText = 'До окончания консультации осталось ' + remainingText;

        $('#alarm').text(alarmText).show();
    }

    // Timer
    setTimeout(updateInfo, updateInterval);
};

// Toggle quick replies block
const toggleQuickReply = (isVisible) => {
    quickReplyIsVisible = isVisible;

    // 
    if (isVisible) {
        $('#quickReplyButton img').attr('src', 'images/chat/down-chevron.svg');
        $('#quickReply').fadeIn('fast');
        $('#quickReply .quick-reply').slideDown('fast', () => $('#filterText').focus());
    } else {
        $('#quickReplyButton img').attr('src', 'images/chat/quick-reply.svg');
        $('#quickReply').fadeOut('fast');
        $('#quickReply .quick-reply').slideUp('fast', () => $('#filterText').val('').trigger('input'));
    }
};

const chatError = (error) => {
    console.error(error.toString());

    blockChat();
};

// Change online status
const changeStatus = (isOnline) => $('#onlineStatus').toggleClass('online', isOnline);

// Send message
const sendMessage = (message) => {
    chatConnection.send('SendMessage', message);

    $('#messageText').val('').trigger('input');
};

// Receive message callback
const onReceiveMessage = (message) => {
    const isSender = message.nickName === 'Член КК № ' + expertKey;

    const li = receiveMessage(message, isSender);

    $('#messagesList').append(li).scrollTo(li);
};

// Join user to chat callback
const onJoinUser = (messageDate, nickName, isFirstJoined, isAppealOnline, isExpertOnline) => {
    changeStatus(isAppealOnline);

    const isSender = nickName === 'Член КК № ' + expertKey;

    const li = joinMessage(messageDate, nickName, isFirstJoined, isSender);

    $("#messagesList").append(li).scrollTo(li);
};

// Leave user from chat callback
const onLeaveUser = (message) => {
    changeStatus(false);

    const isSender = message.nickName === 'Член КК № ' + expertKey;

    const li = leaveMessage(message, isSender);

    $("#messagesList").append(li).scrollTo(li);
};

// First join expert to chat callback
//const onFirstExpert = (expertKey) => {
//    const isSender = nickname !== 'Апеллянт';

//    const li = firstJoinMessage(expertKey, isSender);

//    $("#messagesList").append(li).scrollTo(li);
//};

const onInitializeChange = (messageText) => {
    blockChat();

    const li = changeExpertMessage(messageText);

    $('#messagesList').append(li).scrollTo(li);
};

const blockChat = () => $('#messageForm, #quickReply').remove();

// JQuery document ready (if in range) callback
const onChatReady = () => {
    // 
    $('#sendButton').on('click', () => sendMessage($("#messageText").val()));

    // 
    $('#quickReplyButton').on('click', () => toggleQuickReply(!quickReplyIsVisible));

    // 
    $('#replyList').on('click', 'li', function (e) {
        toggleQuickReply(false);

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
        $(this).expandRows();

        const isDisabled = $(this).val().length === 0;

        $('#sendButton').prop('disabled', isDisabled);
    });

    // 
    $('#messageText').trigger('input');
};

// Create new info hub connection
infoConnection = new signalR.HubConnectionBuilder()
    .withUrl("/info")
    .build();

// Receive information event handler
infoConnection.on("ReceiveInfo", onReceiveInfo);

// JQuery document ready handler
$(document).ready(onChatReady);

// Start info connection
infoConnection.start().then(updateInfo);

// 
getAccessToken(appealId, expertKey).then(accessToken => {
    // Create new chat hub connection
    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chat", { accessTokenFactory: () => accessToken })
        .build();

    // Event handlers

    // Receive message from user handler
    chatConnection.on("Receive", onReceiveMessage);

    // Join user to chat handler
    chatConnection.on("Join", onJoinUser);

    // Leave user from chat handler
    chatConnection.on("Leave", onLeaveUser);

    // First join expert on chat handler
    chatConnection.on("InitializeChange", onInitializeChange);

    // Complete chat
    chatConnection.on("CompleteChat", blockChat);

    // Start chat hub connection
    chatConnection.start();
}).catch(chatError);