const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub") // Địa chỉ của SignalR Hub
    .build();

// Nhận tin nhắn từ server
connection.on("ReceiveMessage", function (user, message) {
    const msg = `${user}: ${message}`;
    const msgElement = document.createElement("div");
    msgElement.innerText = msg;
    document.getElementById("messagesList").appendChild(msgElement);
});

// Kết nối tới SignalR Hub
connection.start().catch(err => console.error(err.toString()));

// Gửi tin nhắn
document.getElementById("sendMessageButton").addEventListener("click", function () {
    const message = document.getElementById("messageInput").value;
    const user = "Customer"; // Đặt tên người dùng hoặc lấy từ session/login
    connection.invoke("SendMessage", user, message).catch(err => console.error(err.toString()));
});
