using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BaiGiuaKy.Models;
using Microsoft.AspNetCore.Authorization;
using BaiGiuaKy.Repositories;
using X.PagedList;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BaiGiuaKy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class OrderListAdminController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;

        public OrderListAdminController(IOrderRepository orderRepository, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
            
        }
        
        public async Task<IActionResult> Index(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            var orders = await _orderRepository.GetAllAsync();
            decimal total = orders.Sum(order => order.TotalPrice); // Tính tổng của tất cả các đơn hàng
            ViewData["Total"] = total; // Truyền tổng sang view


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

            // Retrieve the search string from TempData if available
            searchString = searchString ?? TempData["SearchString"] as string;

            var orders = await _orderRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(p => p.Id.ToString().Contains(searchString));
                decimal total = orders.Sum(order => order.TotalPrice); // Tính tổng của tất cả các đơn hàng
                ViewData["Total"] = total; // Truyền tổng sang view
            }

            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Store the search string in TempData for subsequent requests
            TempData["SearchString"] = searchString;

            return View("Index", await orders.ToPagedListAsync(pageNumber, pageSize));
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                    .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                // Xử lý trường hợp không tìm thấy đơn hàng
                return RedirectToAction("Index");
            }

            // Trả về View với model là order đã tìm được
            return View(order);
        }

    }
}
