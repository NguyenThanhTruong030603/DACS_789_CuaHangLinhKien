using Microsoft.AspNetCore.Mvc;

namespace BaiGiuaky.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
