//$(document).ready(function () {

// Update info in interval
const updateInfo = function (interval, appealId, callback) {
    $.post("update", { appealId }).done(callback);

    //{
    //method: "POST",
    //    url: "update",
    //    //headers: { "Authorization": "Bearer " + accessToken }
    //}
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
    if (element.prop('selectionStart') || element.prop('selectionStart') === '0') {
        var startSubstring = element.val().substring(0, element.prop('selectionStart'));
        var endSubstring = element.val().substring(element.prop('selectionEnd'), element.val().length);

        element.val(startSubstring + value + endSubstring);
    } else {
        element.val(element.val() + value);
    }
};

// Keyup on message textarea
const messageTextKeyup = function (e) {
    if (e.keyCode === 13 && !e.shiftKey) {
        e.preventDefault();

        if ($(this).val().length > 0) sendMessage($(this).val());
    }
};

// Return new "Receive" message
const receiveMessage = (message, isSender) => {
    const nickName = isSender ? 'Вы' : message.nickName;
    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

    const messageInfo = nickName + ' <b class="message-date">' + messageDate.toFormat("tt") + '</b>';

    return addMessage(messageBubble + messageInfo, false, isSender);
};

// Return new "Join user" message
const joinMessage = function (message, isSender) {
    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const messageText = isSender ? 'Вы подключились к консультации' : message.nickName + ' подключился к консультации';

    const html = messageText + ' <b class="message-date">' + messageDate.toFormat("tt") + '</b>';

    return addMessage(html, true, isSender);
};

// Return new "Leave user" message
const leaveMessage = (message, isSender) => {
    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const messageText = isSender ? 'Вы покинули консультацию' : message.nickName + ' покинул консультацию';

    const html = messageText + ' <b class="message-date">' + messageDate.toFormat("tt") + '</b>';

    return addMessage(html, true, isSender);
};

// 
const firstJoinExpertMessage = (nickname, isSender) => {
    const messageText = nickname + ' подключился к консультации. Вы можете задать ему свои вопросы';

    return addMessage(messageText, true, isSender);
};

// 
const changeExpertMessage = (messageText) => addMessage(messageText, true, true);

// Return new list item
const addMessage = (html, isStatusMessage, isSender) => {
    const div = $('<div class="message ' + (isSender ? 'place-left' : 'place-right') + '"></div>').html(html);

    var liElement = $('<li></li>');

    if (isStatusMessage) liElement.addClass('message-status');

    return liElement.html(div);
};

// Scroll to element
jQuery.fn.scrollTo = function (element) {
    $(this).scrollTop($(this).scrollTop() - $(this).offset().top + $(element).offset().top);

    return this;
};

// 
jQuery.fn.insertAtCursor = function (value) {
    if ($(this).prop('selectionStart') || $(this).prop('selectionStart') === '0') {
        var startSubstring = $(this).val().substring(0, $(this).prop('selectionStart'));
        var endSubstring = $(this).val().substring($(this).prop('selectionEnd'), $(this).val().length);

        $(this).val(startSubstring + value + endSubstring);
    } else {
        $(this).val($(this).val() + value);
    }

    return this;
};

// 
jQuery.fn.highlightText = function (match) {
    var value = $(this).text();

    // 
    var matchStart = value.toLowerCase().indexOf(match.toLowerCase());
    // 
    var matchEnd = matchStart + match.length;

    // 
    var beforeMatch = value.slice(0, matchStart);
    // 
    var matchText = value.slice(matchStart, matchEnd);
    // 
    var afterMatch = value.slice(matchEnd);

    $(this).html(beforeMatch + "<strong>" + matchText + "</strong>" + afterMatch);

    //return beforeMatch + "<em>" + matchText + "</em>" + afterMatch;
    return this;
};

jQuery.fn.showModal = function (url, data, duration = 'fast') {
    $.get(url, data, (response) => $(this).html(response).fadeIn(duration));
}
jQuery.fn.hideModal = function (url, data, duration = 'fast') {
    $(this).html('').fadeOut();
}

jQuery.expr.filters.icontains = function (elem, i, m) {
    return (elem.innerText || elem.textContent || "").toLowerCase().indexOf(m[3].toLowerCase()) > -1;
};

// Show modal window
const showModal = (url, data) => $.get(url, data, (response) => $('#modal').html(response).fadeIn('fast'));

// Close modal window
const closeModal = () => $('#modal').html('').fadeOut();

const changeExpert = (accessToken, beforeSendCallback) => {
    return new Promise((resolve, reject) => {
        $.ajax({
            method: "GET",
            url: "api/expert/change",
            headers: { "Authorization": "Bearer " + accessToken },
            beforeSend: beforeSendCallback,
            success: resolve,
            error: reject
        });
    });
};

// Calculate textarea rows by content length
const calculateExpandRows = (textarea) => {
    const minRows = textarea.dataset.minRows | 1;

    textarea.rows = minRows;

    const currentRowCount = Math.ceil((textarea.scrollHeight - textarea.clientHeight) / 16);

    return Math.max(0, Math.min(currentRowCount, 4));
};
//});

var helpInfoIsVisible = false;

const switchHelpInfo = function (isVisible) {
    helpInfoIsVisible = isVisible;

    var offset = { top: 0, left: 0 };

    if (isVisible) {
        const parent = $('#contactsTooltip').parent();

        const tooltipTop = parent.position().top + parent.outerHeight() - $('#contactsTooltip').outerHeight();

        const tooltipLeft = parent.position().left + parent.outerWidth() + 20;

        offset = { top: tooltipTop, left: tooltipLeft };
    }

    $('#contactsTooltip').toggle(isVisible).css(offset);
};

var sideMenuIsVisible = false;

const switchSideMenu = (isVisible) => {
    sideMenuIsVisible = isVisible;

    $('#sideMenu').toggle(isVisible);
};

$(document).ready(function (e) {
    $('#contactsButton').on('click', () => switchHelpInfo(!helpInfoIsVisible));

    $('#closeHelpButton').on('click', () => switchHelpInfo(false));

    $('#sideMenuButton').on('click', () => switchSideMenu(!sideMenuIsVisible));

    $('#closeSideButton').on('click', () => switchSideMenu(false));

    $('#appealInfoButton').on('click', () => $('modal').showModal('ajax/appealinfo', appealId));
});