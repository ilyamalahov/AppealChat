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
        blockChat();

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
        $('#quickReply .quick-reply').slideDown('fast', () => $('#filterText').focus().trigger('input'));
    } else {
        $('#quickReplyButton img').attr('src', 'images/chat/quick-reply.svg');
        $('#quickReply').fadeOut('fast');
        $('#quickReply .quick-reply').slideUp('fast', () => $('#filterText').val(''));
    }
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

    const messageItem = receiveMessage(message, isSender);

    $('#messagesList').append(messageItem).scrollTo(messageItem);
};

// Join user to chat callback
const onJoinUser = (messageDate, nickName, isAppealOnline, isExpertOnline) => {
    changeStatus(isAppealOnline);

    const isSender = nickName === 'Член КК № ' + expertKey;

    const messageItem = joinMessage(messageDate, nickName, isSender);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

// Leave user from chat callback
const onLeaveUser = (message) => {
    changeStatus(false);

    const isSender = message.nickName === 'Член КК № ' + expertKey;

    const messageItem = leaveMessage(message, isSender);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

// First join expert to chat callback
const onFirstJoinExpert = (expertKey) => {
    const messageItem = firstJoinMessage(expertKey, true);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
}

//
const onInitializeChange = (messageText) => {
    blockChat();

    const messageItem = changeExpertMessage(messageText);

    $('#messagesList').append(messageItem).scrollTo(messageItem);
};

// 
const onCompleteChat = () => {
    blockChat();

    const messageItem = completeChatMessage(true);

    $('#messagesList').append(messageItem).scrollTo(messageItem);
};

// 
const blockChat = () => {
    // Stop info hub connection
    //infoConnection.stop();

    // Stop chat hub connection
    //chatConnection.stop();

    // 
    $('#messageForm, #quickReply').remove();
};

// 
const insertQuickReply = (replyText) => {
    toggleQuickReply(false);

    $('#messageText').insertAtCursor(replyText)
        .focus()
        .trigger('input');
};

// 
const onFilterTextKeyup = (e) => {
    const selectedItem = $('#replyList .selected');

    switch (e.keyCode) {
        case 13:
            insertQuickReply(selectedItem.text());

            return;
        case 38:
            const previousItem = selectedItem.prevAll(':visible').first();

            //
            if (previousItem.length === 0) return;

            // 
            selectedItem.removeClass('selected');

            // 
            previousItem.addClass('selected');

            return;
        case 40:
            const nextItem = selectedItem.nextAll(':visible').first();

            //
            if (nextItem.length === 0) return;

            // 
            selectedItem.removeClass('selected');

            // 
            nextItem.addClass('selected');

            return;
    }
};

//
const onFilterTextInput = (e) => {
    const value = $(e.target).val();

    $("#replyList li")
        .removeClass('selected')
        .hide()
        .filter(":icontains('" + value + "')")
        .show()
        .each((index, element) => $(element).highlightText(value))
        .first()
        .addClass('selected');
};

// 
const onMessageTextKeyup = (e) => {
    if (e.keyCode === 13 && !e.shiftKey) {
        e.preventDefault();

        const value = $(e.target).val();

        if (value.length > 0) { sendMessage(value); }
    }
};

//
const onMessageTextInput = function (e) {
    $(this).expandRows();

    const isDisabled = $(this).val().length === 0;

    $('#sendButton').prop('disabled', isDisabled);
};

// Create new info hub connection
infoConnection = new signalR.HubConnectionBuilder()
    .withUrl("/info")
    .build();

// Receive information event handler
infoConnection.on("ReceiveInfo", onReceiveInfo);

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

    // Join user to chat handler
    chatConnection.on("FirstJoinExpert", onFirstJoinExpert);

    // First join expert on chat handler
    chatConnection.on("InitializeChange", onInitializeChange);

    // Complete chat
    chatConnection.on("CompleteChat", onCompleteChat);

    // Start chat hub connection
    chatConnection.start();
}).catch(error => {
        console.error(error.toString());

        blockChat();
    });

// JQuery document ready
$(document).ready(() => {
    // 
    $('#sendButton').on('click', () => sendMessage($("#messageText").val()));

    // 
    $('#quickReplyButton').on('click', () => toggleQuickReply(!quickReplyIsVisible));

    // 
    $('#replyList li').on('click', (e) => insertQuickReply($(e.target).text()));

    // 
    $('#filterText')
        .on('keyup', onFilterTextKeyup)
        .on('input', onFilterTextInput);

    //
    $('#messageText')
        .on('keyup', onMessageTextKeyup)
        .on('input', onMessageTextInput)
        .trigger('input');
});