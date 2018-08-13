//$(document).ready(function () {

// Update info in interval
const updateInfo = function (interval, accessToken, callback) {
    $.ajax({
        method: "POST",
        url: "update",
        headers: { "Authorization": "Bearer " + accessToken }
    }).done(callback);
};

// Get JWT access token
const getAccessToken = function () {
    return new Promise((resolve, reject) => {
        var accessToken = sessionStorage.getItem('access_token');

        if (accessToken) {
            return resolve(accessToken);
        }

        $.post("token", { appealId: appealId, key: expertKey })
            .done(function (response) {
                sessionStorage.setItem("access_token", response.accessToken);

                return resolve(response.accessToken);
            })
            .fail(reject);
    });
};

// Insert text in textarea at cursor position
const insertAtCursor = function (element, value) {
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
};

// Return new "Receive" message
const receiveMessage = (message, isSender) => {
    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

    const messageInfo = '<span class="nickname">' + message.nickName + '</span> <b>(' + messageDate.toFormat("F") + ')</b>';

    return addMessage(messageBubble + messageInfo, isSender);
};

// Return new "Join user" message
const joinUser = function (message, isSender, isAppealOnline, isExpertOnline) {
    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const element = '<span class="nickname">' + message.nickName + '</span> подключился к консультации <b>(' + messageDate.toFormat("F") + ')</b>';

    return addMessage(element, isSender);
};

// Return new "Leave user" message
const leaveUser = (message, isSender) => {
    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const element = '<span class="nickname">' + message.nickName + '</span> покинул консультацию <b>(' + messageDate.toFormat("F") + ')</b>';

    return addMessage(element, isSender);
};

// Return new list item
const addMessage = (element, isSender) => {
    const div = $('<div class="message ' + (isSender ? 'place-left' : 'place-right') + '"></div>').html(element);

    return $('<li></li>').html(div);
};

// Scroll to element
jQuery.fn.scrollTo = function (element) {
    $(this).scrollTop($(this).scrollTop() - $(this).offset().top + $(element).offset().top);

    return this;
};

// Show modal window
const showModal = (url, data) => {
    return new Promise((resolve, reject) => {
        $.get(url, data)
            .done((html) => {
                $('#modal').html(html).fadeIn();
                return resolve();
            })
            .fail(reject);
    });
};

// Close modal window
const closeModal = () => {
    $('#modal').html('').fadeOut();
}

//});