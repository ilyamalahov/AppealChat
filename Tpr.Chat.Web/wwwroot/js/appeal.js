// 
var infoConnection;

// 
var chatConnection;

// Update intreval
const interval = 10000;

// 
const onReceiveMessage = (message) => {
    const isSender = message.nickName === 'Апеллянт';

    const li = receiveMessage(message, isSender);

    $('#messagesList').append(li).scrollTo(li);
};

// 
const onJoinUser = (message, isAppealOnline, onlineExpertKey) => {
    changeStatus(onlineExpertKey !== null);

    const isSender = message.nickName === 'Апеллянт';

    if (!isSender) return;

    const li = joinMessage(message, isSender);

    $("#messagesList").append(li).scrollTo(li);
};

// 
const onLeaveUser = (message, onlineExpertKey) => {
    changeStatus(false);

    const isSender = message.nickName === 'Апеллянт';

    if (!isSender) return;

    const li = leaveMessage(message, isSender);

    $("#messagesList").append(li).scrollTo(li);
};

// 
const onFirstExpert = (expertKey) => {
    // Expert text
    const expertText = '№' + onlineExpertKey;

    $('#expertNumber').text(expertText);

    // 
    const isSender = nickname === 'Апеллянт';

    const li = firstExpertMessage(expertKey, isSender);

    $("#messagesList").append(li).scrollTo(li);
};

// 
const onChangeExpert = (expertKey) => {
    waitChange(true);

    const li = changeExpertMessage(expertKey);

    $('#messagesList').append(li).scrollTo(li);
};

// 
const onReceiveInfo = (currentDate, remainingTime, isAlarm, isFinished) => {
    // Redirect on consultation end
    if (isFinished) location.reload();

    // Remaining duration
    var remainingDuration = luxon.Duration.fromMillis(remainingTime);

    // Remaining time text
    var remainingText = remainingDuration.toFormat('hh:mm:ss');

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

    setTimeout(updateInfo, interval);
};

// Send message
const sendMessage = (message) => {
    chatConnection.send('SendMessage', message);

    // 
    $('#messageText').val('').trigger('input');
};

// 
const changeStatus = (isOnline) => $('#onlineStatus').toggleClass('online', isOnline);

// Change expert functionality
const changeExpert = (appeal) => {
    $.ajax({
        method: "POST",
        url: "ajax/expert/change",
        data: { appealId: appeal },
        beforeSend: () => $('#changeButton').prop('disabled', true),
        success: () => waitChange(true),
        error: (error) => {
            alert(error.responseText);

            $('#changeButton').prop('disabled', false);
        }
    });
};

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

getAccessToken(appealId).then(accessToken => {
    // Update info hub connection
    infoConnection = new signalR.HubConnectionBuilder()
        .withUrl("/info")
        .build();

    // Chat hub connection
    chatConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chat", { accessTokenFactory: () => accessToken })
        .build();

    // Receive information response event
    infoConnection.on("ReceiveInfo", onReceiveInfo);

    // Receiving message from user
    chatConnection.on("Receive", onReceiveMessage);

    // Joining user to chat
    chatConnection.on("Join", onJoinUser);

    // Leave user from chat
    chatConnection.on("Leave", onLeaveUser);

    // First join of expert
    chatConnection.on("FirstJoinExpert", onFirstExpert);

    // Change expert
    chatConnection.on("ChangeExpert", onChangeExpert);

    // Start chat connection
    chatConnection.start().catch((error) => console.error(error.toString()));

    // Start info connection
    infoConnection.start().then(updateInfo).catch((error) => console.error(error.toString()));
});

$(document).ready(() => {
    // Send message
    $('#sendButton').on('click', () => sendMessage($('#messageText').val()));

    // 
    $('#changeButton, #mobileSwitchButton').on('click', () => changeExpert(appealId));

    // 
    //$('#completeButton').on('click', () => $('#modal').showModal('ajax/completechat'));

    // Textarea auto rows count
    $('#messageText').on('input', function (e) {
        $(this).expandRows();

        // 
        const isDisabled = $(this).val().length === 0;

        $('#sendButton').prop('disabled', isDisabled);
    });

    // Message textarea keyup event
    $('#messageText').on('keyup', function (e) {
        if (e.keyCode === 13 && !e.shiftKey) {
            e.preventDefault();

            if ($(this).val().length > 0) { sendMessage($(this).val()); }
        }
    });

    // 
    $('#messageText').trigger('input');

    waitChange(isReadonly);
});