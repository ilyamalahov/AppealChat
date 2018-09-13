// Get JWT access token
const getAccessToken = (appeal, expert) => {
    return new Promise((resolve, reject) => {
        const accessToken = sessionStorage.getItem('access_token');

        if (accessToken) { return resolve(accessToken); }

        $.ajax({
            method: "post",
            url: "token",
            data: { appealId: appeal, key: expert },
            success: (response) => {
                sessionStorage.setItem("access_token", response.accessToken);

                return resolve(response.accessToken);
            },
            error: reject
        });
    });
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
const joinMessage = (message, isSender) => {
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

    return this;
};

// Expand textarea rows by content length
jQuery.fn.expandRows = function (textarea) {
    var currentRows = $(this).data('minRows') | 1;

    $(this).attr('rows', currentRows);

    const rowsCount = Math.ceil(($(this).prop('scrollHeight') - $(this).innerHeight()) / 16);

    currentRows += Math.max(0, Math.min(rowsCount, 4));

    $(this).attr('rows', currentRows);

    return this;
};

// Show modal window
jQuery.fn.showModal = function (url, data) {
    $.get(url, data, (response) => $(this).html(response).fadeIn(300));
};

// Hide modal window
jQuery.fn.hideModal = function () {
    $(this).html('').fadeOut(300);
};

// Compare text in case insensitive
jQuery.expr.filters.icontains = function (elem, i, m) {
    return (elem.innerText || elem.textContent || "").toLowerCase().indexOf(m[3].toLowerCase()) > -1;
};

// 
var helpInfoIsVisible = false;

const switchHelpInfo = (isVisible) => {
    helpInfoIsVisible = isVisible;

    //var offset = { top: 0, left: 0 };

    //if (isVisible) {
    //    const parent = $('#contactsTooltip').parent();

    //    const tooltipTop = parent.position().top + parent.outerHeight() - $('#contactsTooltip').outerHeight();

    //    const tooltipLeft = parent.position().left + parent.outerWidth() + 20;

    //    offset = { top: tooltipTop, left: tooltipLeft };
    //}

    $('#contactsTooltip').toggle(isVisible);
};

const switchSideMenu = (isVisible) => $('#sideMenu').toggle(isVisible);

// 
$(document).ready(() => {
    $('#contactsButton').on('click', () => switchHelpInfo(!helpInfoIsVisible));

    $('#closeHelpButton').on('click', () => switchHelpInfo(false));

    $('#sideMenuButton').on('click', () => switchSideMenu(true));

    $('#closeSideButton').on('click', () => switchSideMenu(false));

    $('#appealSidemenuLink').on('click', () => $('#modal').showModal('modal/appealinfo', { appealId }));

    $('#contactsSidemenuLink').on('click', () => $('#modal').showModal('modal/contacts'));

    $('.sidemenu-item').on('click', () => switchSideMenu(false));
});

$(document).on('click', '#closeModalButton', () => $('#modal').hideModal());