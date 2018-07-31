// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    var accessToken = sessionStorage.getItem('access_token');

    if (!accessToken) {
        $.ajax({ method: "POST", url: "token", data: { appealId: appealId, key: expertKey }, async: false })
            .done(function (response) { sessionStorage.setItem("access_token", response.accessToken); })
            .fail(function (xhr, status, error) { console.log(error); });
    }

    //console.log(localStorage.getItem('access_token'));
});