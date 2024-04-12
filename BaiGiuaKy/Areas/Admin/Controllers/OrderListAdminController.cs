using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BaiGiuaKy.Models;
using Microsoft.AspNetCore.Authorization;
using BaiGiuaKy.Repositories;

namespace BaiGiuaKy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderListAdminController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        public OrderListAdminController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllAsync();
            return View(orders);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var orders = await _orderRepository.GetByIdAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
