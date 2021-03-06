﻿// Get JWT access token
//const getAccessToken = (appeal, expert) => {
//    return new Promise((resolve, reject) => {
//        const accessToken = sessionStorage.getItem('access_token');

//        if (accessToken) { return resolve(accessToken); }

//        $.ajax({
//            method: "post",
//            url: "ajax/token",
//            data: { appealId: appeal, expertKey: expert },
//            success: (response) => {
//                sessionStorage.setItem("access_token", response.accessToken);

//                return resolve(response.accessToken);
//            },
//            error: reject
//        });
//    });
//};

// 
const createClient = (appeal, expert) => {
    const clientId = sessionStorage.getItem("clientId");

    return new Promise(resolve => {
        $.ajax({
            url: "ajax/client",
            data: { appealId: appeal, expertKey: expert, clientId: clientId },
            success: (client) => { sessionStorage.setItem("clientId", client); resolve(client) },
            error: (error) => window.location.replace("/error?message=" + error.responseText),
            async: false
        });
    });
};

// Get JWT Bearer access token
const getJwtToken = (appeal, expert, client) => {
    return new Promise((resolve, reject) => {
        $.ajax({
            method: 'get',
            url: 'ajax/token',
            data: { appealId: appeal, expertKey: expert, clientId: client },
            beforeSend: () => console.info('Request new access token'),
            success: resolve,
            error: reject
        });
    });
};

//
const sendDisconnectRequest = (appeal, expert) => {
    $.ajax({
        method: 'GET',
        url: 'ajax/closepage',
        data: { appealId: appeal, expertKey: expert },
        async: false
    });
};

// Return new "Receive" message
const receiveMessage = (message, isSender) => {
    const nickName = isSender ? 'Вы' : message.nickName;

    const messageDate = luxon.DateTime.fromISO(message.createDate);

    const messageBubble = '<div class="message-bubble">' + message.messageString + '</div>';

    const messageInfo = nickName + ' <b class="message-date">' + messageDate.toFormat("tt") + '</b>';

    return addMessage(messageBubble + messageInfo, isSender, false);
};

// Return new "Join user" message
const joinMessage = (messageDate, nickName, isSender) => {
    const messageDateObj = luxon.DateTime.fromISO(messageDate);
    
    const messageText = isSender ? 'Вы подключились к онлайн-чату' : nickName + ' подключился к онлайн-чату';

    const html = messageText + ' <b class="message-date">' + messageDateObj.toFormat("tt") + '</b>';

    return addMessage(html, isSender);
};

// Return new "Leave user" message
const leaveMessage = (messageDate, nickName, isSender) => {
    const messageDateObj = luxon.DateTime.fromISO(messageDate);

    const messageText = isSender ? 'Вы покинули онлайн-чат' : nickName + ' покинул онлайн-чат';

    const html = messageText + ' <b class="message-date">' + messageDateObj.toFormat("tt") + '</b>';

    return addMessage(html, isSender);
};

// 
const firstJoinMessage = (expertKey, isSender) => {
    const messageText = 'Член КК № ' + expertKey + ' подключился к онлайн-чату. Вы можете задать ему свои вопросы';

    return addMessage(messageText, isSender);
};

    //const messageText = 'Произведена замена члена КК № ' + expertKey;

// Return "Change expert" message
const changeExpertMessage = (messageText) => addMessage(messageText, true);

// Return "Change expert" message
const completeChatMessage = (isSender) => {
    const messageText = 'Апеллянт досрочно завершил онлайн-чат';

    return addMessage(messageText, isSender);
};

// Return new list item
const addMessage = (html, isSender, isStatusMessage = true) => {
    const div = $('<div class="message ' + (isSender ? 'place-left' : 'place-right') + '">').html(html);

    var liElement = $('<li>');

    if (isStatusMessage) liElement.addClass('message-status');

    return liElement.html(div);
};

// Scroll to element
jQuery.fn.scrollTo = function (element) {
    if (element.length == 0) { return; }

    $(this).scrollTop($(this).scrollTop() - $(this).offset().top + $(element).offset().top);

    return this;
};

// 
jQuery.fn.insertAtCursor = function (value) {
    const selectionStart = $(this).prop('selectionStart');
    const selectionEnd = $(this).prop('selectionEnd');
    const currentValue = $(this).val();

    if (selectionStart || selectionStart === '0') {
        var startSubstring = currentValue.substring(0, selectionStart);
        var endSubstring = currentValue.substring(selectionEnd, currentValue.length);

        $(this).val(startSubstring + value + endSubstring);
    } else {
        $(this).val(currentValue + value);
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

    $(this).html(beforeMatch + "<em>" + matchText + "</em>" + afterMatch);

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

// 
jQuery.fn.scrollToLast = function () {
    const lastItem = $(this).children(':last-child');

    $(this).scrollTo(lastItem);

    return this;
}

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
var contactsInfoIsVisible = false;

const toggleHelpInfo = (isVisible) => {
    contactsInfoIsVisible = isVisible;

    // 
    $('#contactsChevron').toggleClass('flipped', isVisible);

    //
    $('#contactsArrow').css({ top: $('#contactsChevron').position().top });

    //
    const leftPosition = $('#contactsButton').outerWidth() + 10;

    //
    $('#contactsTooltip').toggle(isVisible).offset({ 'left': leftPosition });
};

const toggleSideMenu = (isVisible) => $('#sideMenu').toggle(isVisible);

// 
$(document).ready(() => {
    $('#contactsButton').on('click', () => toggleHelpInfo(!contactsInfoIsVisible));
    $('#closeHelpButton').on('click', () => toggleHelpInfo(false));

    $('#sideMenuButton').on('click', () => toggleSideMenu(true));
    $('#closeSideButton').on('click', () => toggleSideMenu(false));

    $('#appealInfoLink').on('click', () => $('#modal').showModal('modal/appealinfo', { appealId }));

    $('#contactsLink').on('click', () => $('#modal').showModal('modal/contacts'));

    $('.sidemenu-item').on('click', () => toggleSideMenu(false));
});

$(document).on('click', '#closeModalButton, #cancelButton, #cancelMobileButton', () => $('#modal').hideModal());