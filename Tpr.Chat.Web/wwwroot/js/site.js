// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    var accessToken = localStorage.getItem('access_token');

    if (!accessToken) {
        $.ajax({ method: "POST", url: "token", data: { appealId: appealId }, async: false })
            .done(function (response) { localStorage.setItem("access_token", response.accessToken); })
            .fail(function (xhr, status, error) { console.log(error); });
    }

    //console.log(localStorage.getItem('access_token'));
});