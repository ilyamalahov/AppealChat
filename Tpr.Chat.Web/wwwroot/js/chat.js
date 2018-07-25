$(document).ready(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chat?access_token=" + localStorage.getItem("access_token"))
        .configureLogging(signalR.LogLevel.Trace)
        .build();

    connection.on("ReceiveMessage", (message, user) => {
        const encodedMsg = user + " says " + message;
        
        const li = document.createElement("li");

        li.textContent = encodedMsg;

        $("#messagesList").appendChild(li);
    });

    connection.start().catch(err => console.error(err));

    $('#chatForm').on('submit', event => {
        event.preventDefault();

        var message = $('#messageInput').val();

        connection.invoke("SendMessage", message)
            .catch(error => console.error(error.toString()));
    });
});