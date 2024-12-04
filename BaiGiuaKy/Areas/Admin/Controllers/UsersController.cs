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
   // [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Get current roles of the user
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove the user from all current roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add the new role to the user
            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (result.Succeeded)
            {
                // Successfully changed the role
                return RedirectToAction("Index");
            }
            else
            {
                // Failed to change the role
                return BadRequest("Failed to change role.");
            }
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> BlockAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index(int? page)
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>(); // Dictionary to store user roles
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user); // Get roles for each user
            }
            // Pass userRoles to the view
            ViewData["UserRoles"] = userRoles;
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
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> UnblockAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = false;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
       
    }
}