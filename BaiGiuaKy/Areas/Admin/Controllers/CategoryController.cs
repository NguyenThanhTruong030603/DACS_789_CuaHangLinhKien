using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BaiGiuaKy.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
	public class CategoryController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Hiển thị danh sách sản phẩm
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
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
    }
}
