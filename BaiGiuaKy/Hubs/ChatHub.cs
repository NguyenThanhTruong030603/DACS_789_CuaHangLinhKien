using Microsoft.AspNetCore.SignalR;

namespace BaiGiuaky.Hubs
{
    public class ChatHub : Hub
    {
        // Hàm server nhận tin nhắn từ client và gửi lại cho tất cả các client
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
