﻿using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace BaiGiuaKy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	public class CategoryController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;

        public CategoryController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        [HttpGet]
        public IActionResult AutocompleteSearch(string term)
        {
            var category = _context.Categories
                .Where(c => c.Name.Contains(term))
                .Select(c => c.Name)
                .ToList();
            return Ok(category);
        }
        // Hiển thị danh sách sản phẩm

        public async Task<IActionResult> Index(int? page)
        {
            ViewData["Title"] = "Trang Chủ";

            var categories = await _categoryRepository.GetAllAsync();



            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(await categories.ToPagedListAsync(pageNumber, pageSize));
        }
        // Hiển thị form thêm sản phẩm mới

        public async Task<IActionResult> Add()
        {         
            return View();
        }
        // Xử lý thêm sản phẩm mới
        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            await _categoryRepository.AddAsync(category);     
            return RedirectToAction(nameof(Index));
        }

        // Hiển thị thông tin chi tiết sản phẩm
        [AllowAnonymous]
        public async Task<IActionResult> Display(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        // Hiển thị form cập nhật sản phẩm

        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {          
            if (ModelState.IsValid)
            {

                var existingProduct = await _categoryRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync
              
                // Cập nhật các thông tin khác của sản phẩm
                existingProduct.Name = category.Name;              

                await _categoryRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();       
            return View(category);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> Search(string searchString, int? page)
        {
            ViewData["Title"] = "Tìm kiếm";

            // Retrieve the search string from TempData if available
            searchString = searchString ?? TempData["SearchString"] as string;

            var categories = await _categoryRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(p => p.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Store the search string in TempData for subsequent requests
            TempData["SearchString"] = searchString;

            return View("Index", await categories.ToPagedListAsync(pageNumber, pageSize));
        }

    }
}
