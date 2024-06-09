using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace BaiGiuaKy.Areas.Admin.Controllers
{
    [Area("Admin")]
   // [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDiscountRepository _discountRepository;
        public DiscountController(IDiscountRepository discountRepository, ApplicationDbContext context)
        {
            _discountRepository = discountRepository;
            _context = context;
        }
        public async Task<IActionResult> Index(int? page)
        {

            var discounts = await _discountRepository.GetAllAsync();

            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(await discounts.ToPagedListAsync(pageNumber, pageSize));
        }
        public async Task<IActionResult> Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(Discount discount)
        {
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            await _discountRepository.AddAsync(discount);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }
        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Discount discount)
        {
            if (ModelState.IsValid)
            {

                var existingDiscount = await _discountRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync

                // Cập nhật các thông tin khác của sản phẩm
                existingDiscount.Code = discount.Code;

                await _discountRepository.UpdateAsync(existingDiscount);

                return RedirectToAction(nameof(Index));
            }
            var discounts = await _discountRepository.GetAllAsync();
            return View(discounts);
        }
        [HttpGet]
        public IActionResult AutocompleteSearch(string term)
        {
            var discount = _context.Discounts
                .Where(c => c.Code.Contains(term))
                .Select(c => c.Code)
                .ToList();
            return Ok(discount);
        }
        public async Task<IActionResult> Search(string searchString, int? page)
        {
            ViewData["Title"] = "Tìm kiếm";

            // Retrieve the search string from TempData if available
            searchString = searchString ?? TempData["SearchString"] as string;

            var discounts = await _discountRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                discounts = discounts.Where(p => p.Code.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Store the search string in TempData for subsequent requests
            TempData["SearchString"] = searchString;

            return View("Index", await discounts.ToPagedListAsync(pageNumber, pageSize));
        }

       
    }
}
