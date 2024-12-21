using System.Diagnostics;
using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System;
using BaiGiuaKy.Extensions;
using Microsoft.AspNetCore.Identity;

namespace BaiGiuaKy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult AutocompleteSearch(string term)
        {
            var product = _context.Products
                .Where(c => c.Name.Contains(term))
                .Select(c => c.Name)
                .ToList();
            return Ok(product);
        }
        public async Task<IActionResult> Index( int? page, string sort)
        {
            ViewData["Title"] = "Trang Ch?";

            var products = await _productRepository.GetAllAsync();
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            switch (sort)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name).ToList();
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    products = products.OrderBy(p => p.Name).ToList(); // Sắp xếp mặc định theo tên tăng dần
                    break;
            }

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(await products.ToPagedListAsync(pageNumber, pageSize));
        }
        public IActionResult about()
        {
            return View();
        }

		public IActionResult Contact()
		{
			return View();
		}

        [AllowAnonymous]
        public async Task<IActionResult> Display(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            ViewData["UserManager"] = _userManager;
            var product = await _context.Products
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [AllowAnonymous]
        public async Task<IActionResult> Search(string searchString, int? page)
        {
            ViewData["Title"] = "Tìm ki?m";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            //Truy xu?t chu?i tìm ki?m t? TempData n?u có s?n.
            searchString = searchString ?? TempData["SearchString"] as string;

            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // Store the search string in TempData for subsequent requests
            TempData["SearchString"] = searchString;

            return View("Index", await products.ToPagedListAsync(pageNumber, pageSize));
        }

        public async Task<IActionResult> ShowProductsByCategories(int categoryId, int? page)
        {
            ViewData["Title"] = "San pham theo loai";
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            // L?y t?t c? s?n ph?m thu?c lo?i ???c ch? ??nh
            var products = await _productRepository.GetAllAsync();
            products = products.Where(p => p.CategoryId == categoryId);

            int pageSize = 4;
            int pageNumber = page ?? 1;

            return View("Index", await products.ToPagedListAsync(pageNumber, pageSize));
        }
        public async Task<IActionResult> BuildPC()
        {
         
           
            // Lấy tất cả sản phẩm từ cơ sở dữ liệu
            var product = await _productRepository.GetAllAsync();
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> ShowProductsByCategory(int categoryId)
        {
            // Lấy sản phẩm theo categoryId từ cơ sở dữ liệu
            var filteredProducts = await _productRepository.GetAllAsync();
            var result = filteredProducts.Where(p => p.CategoryId == categoryId).ToList();
            return Json(result);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(int productId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Nội dung bình luận không được để trống.");
                return RedirectToAction("Display", new { id = productId });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var comment = new Comment
            {
                ProductId = productId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Display", new { id = productId });
        }
        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, string content)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound();
            }

			// Kiểm tra nếu người dùng đã đăng nhập và là chủ sở hữu bình luận
			if (!User.Identity.IsAuthenticated ||
	 (!User.IsInRole("Admin") && !User.IsInRole("Employee") && comment.UserId != _userManager.GetUserId(User)))
			{
				return Unauthorized();
			}

			// Cập nhật nội dung bình luận
			comment.Content = content;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Display", new { id = comment.ProductId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound();
            }

			if (!User.Identity.IsAuthenticated ||
	  (!User.IsInRole("Admin") && !User.IsInRole("Employee") && comment.UserId != _userManager.GetUserId(User)))
			{
				return Unauthorized();
			}

			// Xóa bình luận
			_context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Display", new { id = comment.ProductId });
        }
    }
}
