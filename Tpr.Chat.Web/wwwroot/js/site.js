// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    updateInfo = function (interval) {
        $.ajax({
            method: "POST",
            url: "update",
            headers: { "Authorization": "Bearer " + sessionStorage.getItem("access_token") },
            success: function (response) {
                // Current Time
                var currentDate = new Date(response.currentDate);
                $('#moscowTime').text(currentDate.toLocaleTimeString());

                // Remaining Time
                var remainingDate = new Date(response.remainingTime);
                $('#remainingTime').text(remainingDate.toLocaleTimeString());

                // Recursive invoke setTimeout()
                setTimeout(updateInfo, interval, interval);
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    };
});