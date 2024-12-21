using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaiGiuaKy.Repositories;
using BaiGiuaKy.Models;
using Microsoft.EntityFrameworkCore;
using BaiGiuaKy.Extensions;

namespace BaiGiuaKy.Controllers
{
    public class BuildPCController : Controller
    {
        private readonly IProductRepository _productRepository;

        public BuildPCController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
       


        [HttpPost]
        public IActionResult RemoveComponent(string type, int productId)
        {
            // Lấy dữ liệu đã chọn từ TempData và giải mã từ JSON
            var selectedComponentsJson = TempData["SelectedComponents"] as string;
            var selectedComponents = string.IsNullOrEmpty(selectedComponentsJson)
                ? new Dictionary<string, int>()
                : JsonConvert.DeserializeObject<Dictionary<string, int>>(selectedComponentsJson);

            // Kiểm tra xem linh kiện có được chọn không, nếu có thì xóa nó
            if (selectedComponents.ContainsKey(type) && selectedComponents[type] == productId)
            {
                selectedComponents.Remove(type); // Remove the selected component
            }

            // Lưu lại dữ liệu vào TempData dưới dạng JSON
            TempData["SelectedComponents"] = JsonConvert.SerializeObject(selectedComponents);
            TempData.Keep(); // Giữ lại TempData sau khi redirect

            return RedirectToAction("Index"); // Redirect back to the Build PC page
        }



        // Trang chính Build PC
        public async Task<IActionResult> Index()
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
            ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            // Lấy dữ liệu đã chọn từ TempData và giải mã từ JSON
            var selectedComponentsJson = TempData["SelectedComponents"] as string;
            var selectedComponents = string.IsNullOrEmpty(selectedComponentsJson)
                ? new Dictionary<string, int>()
                : JsonConvert.DeserializeObject<Dictionary<string, int>>(selectedComponentsJson);

            TempData.Keep(); // Đảm bảo dữ liệu không bị xóa sau khi đọc

            // Lấy thông tin chi tiết sản phẩm đã chọn từ cơ sở dữ liệu
            var selectedProducts = new List<Product>();
            foreach (var componentId in selectedComponents.Values)
            {
                var product = await _productRepository.GetByIdAsync(componentId);
                if (product != null)
                {
                    selectedProducts.Add(product);
                }
            }

            // Truyền dữ liệu vào View
            ViewBag.SelectedComponents = selectedComponents;
            return View(selectedProducts);  // Truyền danh sách sản phẩm vào View
        }


        // Trang hiển thị sản phẩm theo loại linh kiện
        public async Task<IActionResult> SelectComponent(string type)
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
            ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            var products = await _productRepository.GetByCategoryAsync(type);
            ViewBag.ComponentType = type; // Loại linh kiện (CPU, RAM, VGA, v.v.)
            return View(products);
        }

        // Xử lý khi người dùng chọn linh kiện
        [HttpPost]
        public IActionResult SelectComponent(string type, int productId)
        {
            // Lấy dữ liệu đã chọn từ TempData và giải mã từ JSON
            var selectedComponentsJson = TempData["SelectedComponents"] as string;
            var selectedComponents = string.IsNullOrEmpty(selectedComponentsJson)
                ? new Dictionary<string, int>()
                : JsonConvert.DeserializeObject<Dictionary<string, int>>(selectedComponentsJson);

            // Cập nhật linh kiện đã chọn
            selectedComponents[type] = productId;

            // Lưu lại dữ liệu vào TempData dưới dạng JSON
            TempData["SelectedComponents"] = JsonConvert.SerializeObject(selectedComponents);
            TempData.Keep(); // Giữ lại TempData sau khi redirect

            return RedirectToAction("Index");
        }

    }
}
