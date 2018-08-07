//$(document).ready(function () {
    // Update info in interval
    const updateInfo = function (interval, accessToken) {
        return new Promise((resolve, reject) => {
            $.ajax({ method: "POST", url: "update", headers: { "Authorization": "Bearer " + accessToken } })
                .done(function (response) {
                    setTimeout(updateInfo, interval, interval, accessToken);
                    resolve(response);
                })
                .fail(reject);
        });
    };

    // Convert current timestamp to human-readable date on Moscow timezone
    const toMoscowDate = function (timestamp) {
        var timestampDate = new Date();

        return timestampDate;
    }

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
                .fail(function () { return reject(); });
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

    // Scroll to element
    jQuery.fn.scrollTo = function (element) {
        $(this).scrollTop($(this).scrollTop() - $(this).offset().top + $(element).offset().top);

        return this;
    };
//});