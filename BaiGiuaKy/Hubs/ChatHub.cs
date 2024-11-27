using Microsoft.AspNetCore.SignalR;

namespace BaiGiuaky.Hubs
{
    public class ChatHub : Hub
    {
        // Hàm server nhận tin nhắn từ client và gửi lại cho tất cả các client
        public async Task SendMessage(string user, string message)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                // Nếu người dùng đã đăng nhập, gửi tin nhắn
                await Clients.All.SendAsync("ReceiveMessage", user, message);
            }
            else
            {
                // Nếu người dùng chưa đăng nhập, gửi thông báo
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "Bạn cần đăng nhập để gửi tin nhắn.");
            }
        }
    }
}
