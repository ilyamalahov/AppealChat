const connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

connection.on("ReceiveMessage", (user, message) => {
    const encodedMsg = user + " says " + message;
    const li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(err => console.error(err.toString()));

document.getElementById('chatForm').addEventListener('submit', event => {
    var message = document.getElementById('messageInput').value;
    connection.invoke("SendMessage", message).catch(error => console.error(error.toString()));
    event.preventDefault();
});