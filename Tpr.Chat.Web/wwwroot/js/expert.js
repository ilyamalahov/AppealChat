// Variables

// Quick reply block is visible?
var quickReplyIsVisible = false;

// Update time interval in milliseconds
const updateInterval = 1000;

const tokenInterval = 20000;

// Update info hub connection
var infoConnection = new signalR.HubConnectionBuilder()
    .withUrl("/info")
    .build();

// Chat hub connection
var chatConnection;

// 
var accessToken;

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
    chatConnection.send('SendMessage', appealId, message, expertKey);

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
const onLeaveUser = (messageDate, nickName) => {
    changeStatus(false);

    const isSender = nickName === 'Член КК № ' + expertKey;

    const messageItem = leaveMessage(messageDate, nickName, isSender);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

// First join expert to chat callback
const onFirstJoinExpert = (expert) => {
    const messageItem = firstJoinMessage(expert, true);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

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
const insertReply = (replyText) => {
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
            insertReply(selectedItem.text());

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

// Refresh access token
const refreshToken = (appeal, expert) => getJwtToken(appeal, expert).then(token => { accessToken = token; setTimeout(() => refreshToken(appeal), tokenInterval); });

// Receive information event handler
infoConnection.on("ReceiveInfo", onReceiveInfo);

// JQuery document ready
$(document).ready(() => {
    // Quick reply toggle
    $('#quickReplyButton').on('click', () => toggleQuickReply(!quickReplyIsVisible));

    // Quick reply filter text
    $('#filterText')
        .on('keyup', onFilterTextKeyup)
        .on('input', onFilterTextInput);

    // Quick reply list
    $('#replyList li').on('click', (e) => insertReply($(e.target).text()));
    
    // Message form textarea
    $('#messageText')
        .on('keyup', onMessageTextKeyup)
        .on('input', onMessageTextInput)
        .trigger('input');

    // Message form send button
    $('#sendButton').on('click', () => sendMessage($("#messageText").val()));
});

// Start info connection
infoConnection.start().then(updateInfo);

// 
getJwtToken(appealId, expertKey)
    .then(token => {
        accessToken = token;

        const options = { accessTokenFactory: () => accessToken };

        chatConnection = new signalR.HubConnectionBuilder()
            .withUrl('chat', options)
            .configureLogging(signalR.LogLevel.Trace)
            .build();

        // Receiving message from user
        chatConnection.on("Receive", onReceiveMessage);

        // Joining user to chat
        chatConnection.on("Join", onJoinUser);

        // Leave user from chat
        chatConnection.on("Leave", onLeaveUser);

        // Join user to chat handler
        chatConnection.on("FirstJoinExpert", onFirstJoinExpert);

        // Initialize change expert
        chatConnection.on("InitializeChange", onInitializeChange);

        // Complete change expert
        chatConnection.on("CompleteChat", onCompleteChat);

        // Start chat connection
        return chatConnection.start();
    })
    .then(() => chatConnection.invoke('Join'))
    .then(() => setTimeout(() => refreshToken(appealId), tokenInterval))
    .catch(error => { console.error(error.toString()); blockChat(); });