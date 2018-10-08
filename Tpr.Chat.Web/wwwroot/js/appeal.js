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

const onFirstJoinExpert = (expertKey) => {
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
    refreshToken(appealId);

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
    chatConnection.send('SendMessage', appealId, message);

    // 
    $('#messageText').val('').trigger('input');
};

// 
const scrollToLast = function () {
    const targetHeight = $(this).height();

    if (targetHeight !== originalHeight) {
        const lastItem = $('#messagesList').children(':last-child');

        $('#messagesList').scrollTo(lastItem);
    }
};

// 
const changeStatus = (isOnline) => $('#onlineStatus').toggleClass('online', isOnline);

// Change expert functionality
const changeExpert = (appeal) => {
    $.ajax({
        method: "POST",
        url: "ajax/change/expert",
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
        method: "POST",
        url: "ajax/chat/complete",
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
        $('#messageForm').hide();

        $('#modal').showModal('modal/waitchange');
    } else {
        $('#messageForm').show();

        $('#modal').hideModal();
    }
};

// 
const updateInfo = () => infoConnection.send("MainUpdate", appealId);

// Refresh access token
const refreshToken = (appeal) => getJwtToken(appeal).then(token => { accessToken = token; setTimeout(() => refreshToken(appeal), tokenInterval); });

// 
const onMessageTextKeyup = function (e) {
    if (e.keyCode === 13 && !e.shiftKey) {
        e.preventDefault();

        if ($(this).val().length > 0) { sendMessage($(this).val()); }
    }
};

// 
const onMessageTextInput = function (e) {
    $(this).expandRows();

    // 
    const isDisabled = $(this).val().length === 0;

    $('#sendButton').prop('disabled', isDisabled);
};

const connectToChat = () => {
    chatConnection.send('Join').then(() => setTimeout(() => refreshToken(appealId), tokenInterval));
};

// Receive information response event
infoConnection.on("ReceiveInfo", onReceiveInfo);

$(window).on('resize', scrollToLast);

// Document ready event
$(document).ready(() => {
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
        .on('keyup', onMessageTextKeyup)
        .on('input', onMessageTextInput)
        .trigger('input');

    //
    waitChange(isWaiting);
});

// Start info connection
infoConnection.start().then(updateInfo);

// Chat connection
getJwtToken(appealId)
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
        chatConnection.on("CompleteChange", onCompleteChange);

        // Start chat connection
        return chatConnection.start();
    })
    .then(() => chatConnection.invoke('Join'))
    .then(() => setTimeout(() => refreshToken(appealId), tokenInterval))
    .catch(error => alert(error.toString()));

//
$(document).on('click', '#okChangeButton, #okChangeMobileButton', () => { $('#modal').hideModal(); changeExpert(appealId); });

//
$(document).on('click', '#okCompleteButton, #okCompleteMobileButton', () => { $('#modal').hideModal(); completeChat(appealId); });