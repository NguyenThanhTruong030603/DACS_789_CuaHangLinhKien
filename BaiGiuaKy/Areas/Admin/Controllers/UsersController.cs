using BaiGiuaKy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using static NuGet.Packaging.PackagingConstants;

namespace BaiGiuaKy_Nhom3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult AutocompleteSearch(string term)
        {
            var user = _userManager.Users
                .Where(c => c.Email.Contains(term))
                .Select(c => c.Email)
                .ToList();
            return Ok(user);
        }
        public async Task<IActionResult> Index(int? page)
        {
            var users = await _userManager.Users.ToListAsync();

            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(await users.ToPagedListAsync(pageNumber, pageSize));
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Reset password to "Hutech@123"
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, "Hutech@123");
            if (result.Succeeded)
            {
                // Password reset successfully
                // You can redirect to a success page or do something else
                return RedirectToAction("Index");
            }
            else
            {
                // Password reset failed
                // You can handle the failure scenario accordingly
                return BadRequest("Password reset failed.");
            }
        }
        public async Task<IActionResult> Search(string searchString, int? page)
        {
            ViewData["Title"] = "Tìm kiếm";

            // Lấy chuỗi tìm kiếm từ TempData nếu có
            searchString = searchString ?? TempData["SearchString"] as string;

            // Lấy danh sách người dùng từ UserManager
            var users = await _userManager.Users.ToListAsync();

            // Lọc danh sách người dùng dựa trên chuỗi tìm kiếm (nếu có)
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Email.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            int pageSize = 4;
            int pageNumber = page ?? 1;

            // Lưu chuỗi tìm kiếm trong TempData để sử dụng cho các request sau
            TempData["SearchString"] = searchString;

            // Trả về view "Index" với danh sách người dùng đã lọc và phân trang
            return View("Index", await users.ToPagedListAsync(pageNumber, pageSize));
        }

    }
}