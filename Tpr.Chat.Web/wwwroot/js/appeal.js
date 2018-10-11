// Update info hub connection
var infoConnection = new signalR.HubConnectionBuilder()
    .withUrl("info")
    .build();

// 
var chatConnection;

// Jwt Bearer access token
var accessToken;

// Refresh token interval
const tokenInterval = 20000;

// Update intreval
const infoInterval = 10000;

// Original window height
const originalHeight = $(window).height();

// 
const onReceiveMessage = (message) => {
    const isSender = message.nickName === 'Апеллянт';

    const messageItem = receiveMessage(message, isSender);

    $('#messagesList').append(messageItem).scrollTo(messageItem);
};

// 
const onJoinUser = (messageDate, nickName, isAppealOnline, isExpertOnline) => {
    changeStatus(isExpertOnline);

    const isSender = nickName === 'Апеллянт';

    if (!isSender) { return; }

    // 
    const messageItem = joinMessage(messageDate, nickName, isSender);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

// 
const onLeaveUser = (messageDate, nickName) => {
    changeStatus(false);

    const isSender = nickName === 'Апеллянт';

    if (!isSender) { return; }

    const messageItem = leaveMessage(messageDate, nickName, isSender);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

const onFirstJoinExpert = (expertKey, isAppealOnline, isExpertOnline) => {
    changeStatus(isExpertOnline);

    // Expert text
    const expertText = '№' + expertKey;

    $('#expertNumber').text(expertText);

    // 
    const messageItem = firstJoinMessage(expertKey, false);

    $("#messagesList").append(messageItem).scrollTo(messageItem);
};

// 
const onInitializeChange = (messageText) => {
    const messageItem = changeExpertMessage(messageText);

    $('#messagesList').append(messageItem).scrollTo(messageItem);
};

// 
const onCompleteChange = (expertKey) => {
    //refreshToken(appealId);

    waitChange(false);
};

// 
const onReceiveInfo = (currentDate, remainingTime, isAlarm, isFinished) => {
    // Redirect on consultation end
    if (isFinished) { location.reload(); }

    // Remaining duration
    var remainingDuration = luxon.Duration.fromMillis(remainingTime);

    // Remaining time text
    var remainingText = remainingDuration.toFormat('mm:ss');

    // Remaining format minutes
    var remainingMinutes = remainingDuration.as("minutes");

    if (remainingMinutes < 1) { remainingText = 'меньше'; }

    $('#remainingTime, #mobileRemainingTime').text(remainingText);

    // Moscow date
    var moscowDate = luxon.DateTime.fromISO(currentDate, { zone: 'utc+3' });

    $('#moscowTime').text(moscowDate.toFormat('t'));

    // Alarm after minutes (5 minutes default)
    if (isAlarm) {
        var alarmText = 'До окончания консультации осталось ' + remainingText + ' минут(-ы)';

        $('#alarm').text(alarmText).show();
    }

    setTimeout(updateInfo, infoInterval);
};

// Send message
const sendMessage = (message) => {
    chatConnection.send('SendMessage', message);

    // 
    $('#messageText').val('').trigger('input');
};

const onWindowResize = (e) => {
    const targetHeight = $(e.target).height();

    if (targetHeight !== originalHeight) { $('#messagesList').scrollToLast(); }
}

// 
const changeStatus = (isOnline) => $('#onlineStatus').toggleClass('online', isOnline);

// Change expert functionality
const changeExpert = (appeal) => {
    $.ajax({
        method: "post",
        url: "ajax/change",
        data: { appealId: appeal },
        beforeSend: () => $('#changeButton, #changeMobileButton').prop('disabled', true),
        success: () => waitChange(true),
        error: (error) => {
            alert(error.responseText);

            $('#changeButton, #changeMobileButton').prop('disabled', false);
        }
    });
};

// 
const completeChat = (appeal) => {
    $.ajax({
        method: "post",
        url: "ajax/complete",
        data: { appealId: appeal },
        beforeSend: () => $('#completeButton, #completeMobileButton').prop('disabled', true),
        success: () => location.reload(),
        error: (error) => {
            alert(error.responseText);

            $('#completeButton, #completeMobileButton').prop('disabled', false);
        }
    });
};

// 
const waitChange = (isWait) => {
    if (isWait) {
        infoConnection.stop();

        $('#messageForm').hide();

        $('#modal').showModal('modal/waitchange');
    } else {
        infoConnection.start().then(updateInfo);

        $('#messageForm').show();

        $('#modal').hideModal();
    }
};

// 
const updateInfo = () => infoConnection.send("MainUpdate", appealId);

// Refresh access token
const refreshToken = () => {
    const clientId = sessionStorage.getItem('clientId');

    getJwtToken(appealId, null, clientId).then(token => { accessToken = token; setTimeout(refreshToken, tokenInterval); });
};

// 
const onMessageTextKeydown = (e) => {
    if (e.keyCode === 13 && !e.shiftKey) {
        e.preventDefault();

        const value = $(e.target).val();

        if (value.length > 0) { sendMessage(value); }
    }
};

// 
const onMessageTextInput = function (e) {
    $(this).expandRows();

    // 
    const isDisabled = $(this).val().length === 0;

    $('#sendButton').prop('disabled', isDisabled);
};

// 
const completeLoad = () => {
    // 
    $('#messagesList').scrollToLast();
    
    // 
    setTimeout(refreshToken, tokenInterval);
};

// Receive information response event
infoConnection.on("ReceiveInfo", onReceiveInfo);

const sleep = (ms) => {
    return new Promise(resolve => setTimeout(resolve, ms));
}

// Document ready event
$(document).ready(() => {
    // 
    $(window).on('resize', onWindowResize);

    // Send message
    $('#sendButton').on('click', () => sendMessage($('#messageText').val()));

    // 
    $('#sideMobileButton').on('click', () => toggleSideMenu(true));

    // 
    $('#changeButton, #mobileChangeButton').on('click', () => $('#modal').showModal('modal/changeexpert'));

    //
    $('#completeButton, #completeMobileButton').on('click', () => $('#modal').showModal('modal/completechat'));

    // Textarea auto rows count
    $('#messageText')
        .on('keydown', onMessageTextKeydown)
        .on('input', onMessageTextInput)
        .trigger('input');

    //
    waitChange(isWaiting);
});

createClient(appealId)
    .then(clientId => getJwtToken(appealId, clientId))
    .then(token => {
        // Set access token
        accessToken = token;

        // Chat connection
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
        chatConnection.on("CompleteChange", onCompleteChange);

        // Start chat connection
        return chatConnection.start();
    })
    .then(() => chatConnection.send('Join'))
    .then(completeLoad)
    .catch(error => alert(error.toString()));

//
$(document).on('click', '#okChangeButton, #okChangeMobileButton', () => { $('#modal').hideModal(); changeExpert(appealId); });

//
$(document).on('click', '#okCompleteButton, #okCompleteMobileButton', () => { $('#modal').hideModal(); completeChat(appealId); });