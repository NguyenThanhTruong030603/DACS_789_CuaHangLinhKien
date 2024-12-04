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
    // [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class OrderListAdminController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;

        public OrderListAdminController(IOrderRepository orderRepository, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;

        }
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int orderId, string newStatus)
        {
            // Chuyển đổi giá trị newStatus từ chuỗi thành enum OrderStatus
            if (!Enum.TryParse<OrderStatus>(newStatus, out OrderStatus status))
            {
                // Nếu không thể chuyển đổi, trả về BadRequest hoặc xử lý theo cách khác
                return BadRequest("Trạng thái không hợp lệ");
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái mới cho đơn hàng
            order.Status = status;
            await _orderRepository.UpdateAsync(order);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            // Lấy danh sách đơn hàng có trạng thái "Chờ xác nhận" từ repository
            var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ChờXácNhận);

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            // Thiết lập phân trang
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Trả về view Index với danh sách đơn hàng đã lấy được
            return View(await orders.ToPagedListAsync(pageNumber, pageSize));
        }
        public async Task<IActionResult> IndexXacNhan(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            // Lấy danh sách đơn hàng có trạng thái "Chờ xác nhận" từ repository
            var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãXácNhận);

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            // Thiết lập phân trang
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Trả về view Index với danh sách đơn hàng đã lấy được
            return View(await orders.ToPagedListAsync(pageNumber, pageSize));
        }

        public async Task<IActionResult> IndexDangGiao(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            // Lấy danh sách đơn hàng có trạng thái "Chờ xác nhận" từ repository
            var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãGiao);

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            // Thiết lập phân trang
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Trả về view Index với danh sách đơn hàng đã lấy được
            return View(await orders.ToPagedListAsync(pageNumber, pageSize));
        }
        public async Task<IActionResult> IndexHuy(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            // Lấy danh sách đơn hàng có trạng thái "Chờ xác nhận" từ repository
            var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãHủy);

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            // Thiết lập phân trang
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Trả về view Index với danh sách đơn hàng đã lấy được
            return View(await orders.ToPagedListAsync(pageNumber, pageSize));
        }
        public async Task<IActionResult> IndexGiaoThanhCong(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            // Lấy danh sách đơn hàng có trạng thái "Chờ xác nhận" từ repository
            var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãGiaoThànhCông);

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            // Thiết lập phân trang
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Trả về view Index với danh sách đơn hàng đã lấy được
            return View(await orders.ToPagedListAsync(pageNumber, pageSize));
        }
        ////////////////////////////////////////////////////////////////////////////////
        public async Task<IActionResult> ThongKeView(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            // Lấy danh sách đơn hàng có trạng thái "Chờ xác nhận" từ repository
            var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãGiaoThànhCông);

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            // Thiết lập phân trang
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Trả về view Index với danh sách đơn hàng đã lấy được
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

        public async Task<IActionResult> ThongKe(DateTime startDate, DateTime endDate)
        {
            ViewData["Title"] = "Tìm kiếm";

            // Lấy danh sách đơn hàng đã giao thành công trong khoảng thời gian
            var orders = await _orderRepository.GetOrdersByStatusAsync2(OrderStatus.ĐãGiaoThànhCông);
            orders = orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            // Lọc và tính tổng tiền theo từng tháng
            var months = orders
                .GroupBy(o => o.OrderDate.Month) // Group theo tháng
                .Select(g => g.Key)
                .ToList();

            var totalAmounts = orders
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => g.Sum(o => o.TotalPrice))
                .ToList();

            // Truyền dữ liệu cho View
            ViewData["Months"] = months;
            ViewData["TotalAmounts"] = totalAmounts;

            // Tính tổng giá trị của các đơn hàng
            decimal total = orders.Sum(order => order.TotalPrice);
            ViewData["Total"] = total;

            return View("ThongKe", orders.ToPagedList(1, 4)); // Trả về trang IndexGiaoThanhCong
        }

        public async Task<IActionResult> Search(string searchString, int? page, string viewName)
        {
            ViewData["Title"] = "Tìm kiếm";

            // Retrieve the search string from TempData if available
            searchString = searchString ?? TempData["SearchString"] as string;

            // Kiểm tra viewName và lấy danh sách đơn hàng tương ứng
            IEnumerable<Order> orders;

            if (viewName == "Index")
            {
                orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ChờXácNhận);
            }
            else if (viewName == "IndexGiaoThanhCong")
            {
                orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãGiaoThànhCông);
            }
            else if (viewName == "IndexXacNhan")
            {
                orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãXácNhận);
            }
            else if (viewName == "IndexHuy")
            {
                orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãHủy);
            }
            else if (viewName == "IndexDangGiao")
            {
                orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ĐãGiao);
            }
            else
            {
                // Trường hợp viewName không xác định, xử lý lỗi hoặc mặc định là Index
                orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.ChờXácNhận);
                viewName = "Index"; // Đặt viewName mặc định là Index
            }

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

            return View(viewName, await orders.ToPagedListAsync(pageNumber, pageSize));
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.ApplicationUser)
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
