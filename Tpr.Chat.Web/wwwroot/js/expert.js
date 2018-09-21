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

const blockChat = () => {
    // Stop info hub connection
    infoConnection.stop();

    // Stop chat hub connection
    chatConnection.stop();

    // 
    $('#messageForm, #quickReply').remove();
};

// JQuery document ready (if in range) callback
const onChatReady = () => {
    // 
    $('#sendButton').on('click', () => sendMessage($("#messageText").val()));

    // 
    $('#quickReplyButton').on('click', () => toggleQuickReply(!quickReplyIsVisible));

    //
    //$("#replyList").selectable({
    //    selected: (event, item) => {
    //        console.log($(item).text());
    //    }
    //});

    const insertQuickReply = (replyText) => {
        toggleQuickReply(false);

        $('#messageText').insertAtCursor(replyText)
            .focus()
            .trigger('input');
    };


    
    $('#replyList li').on('click', function () {
        toggleQuickReply(false);

        insertQuickReply($(this).text());
    });

    $('#filterText').on('keyup', (e) => {
        const selectedItem = $('#replyList .selected');

        var targetItem = selectedItem.closest('li:visible');

        switch (e.keyCode) {
            case 13:
                insertQuickReply(selectedItem.text());
                return;
            case 38:
                const previousItem = selectedItem.prevAll(':visible').first();

                console.log(previousItem.length);
                if (previousItem.length === 0) return;

                // 
                selectedItem.removeClass('selected');

                // 
                previousItem.addClass('selected');
                return;
            case 40: const nextItem = selectedItem.nextAll(':visible').first();

                console.log(nextItem.length);
                if (nextItem.length === 0) return;

                // 
                selectedItem.removeClass('selected');

                // 
                nextItem.addClass('selected');
                return;
        }
        //if (e.keyCode == 38) {
            

        //    return;
        //}else if (e.keyCode == 40) {
            

        //    return;
        //}
    });

    //
    $('#filterText').on('input', function (e) {
        var value = $(this).val();

        $("#replyList li")
            .removeClass('selected')
            .hide()
            .filter(":icontains('" + value + "')")
            .show()
            .each((index, element) => $(element).highlightText(value))
            .first()
            .addClass('selected');
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