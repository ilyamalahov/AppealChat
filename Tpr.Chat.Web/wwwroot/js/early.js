$(document).ready(function () {
    var accessToken = sessionStorage.getItem('access_token');

    if (!accessToken) {
        $.post({
            method: "POST",
            url: "token",
            data: { appealId: appealId, key: expertKey },
            success: function (response) {
                sessionStorage.setItem("access_token", response.accessToken);

                updateInfo
            },
            error: function (xhr, status, error) {
                window.location.href = "/error";
            },
            async: false
        })
    }
});