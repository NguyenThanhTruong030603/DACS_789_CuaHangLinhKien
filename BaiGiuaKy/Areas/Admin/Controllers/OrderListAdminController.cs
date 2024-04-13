using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BaiGiuaKy.Models;
using Microsoft.AspNetCore.Authorization;
using BaiGiuaKy.Repositories;
using X.PagedList;

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
        public async Task<IActionResult> Index(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            var orders = await _orderRepository.GetAllAsync();



            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(await orders.ToPagedListAsync(pageNumber, pageSize));
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

        public async Task<IActionResult> Search(string searchString, int? page)
        {
            ViewData["Title"] = "Tìm kiếm";

            var orders = await _orderRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(p => p.Id.ToString().Contains(searchString));
            }

            int pageSize = 4;
            int pageNumber = page ?? 1;
            return View("Index", await orders.ToPagedListAsync(pageNumber, pageSize));
        }
    }
}
