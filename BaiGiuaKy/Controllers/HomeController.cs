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

namespace BaiGiuaKy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public HomeController(IProductRepository productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
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
        public async Task<IActionResult> Index( int? page)
        {
            ViewData["Title"] = "Trang Ch?";

            var products = await _productRepository.GetAllAsync();
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);


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
            var product = await _productRepository.GetByIdAsync(id);
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

        public async Task<IActionResult> ShowProductsByCategory(int categoryId, int? page)
        {
            ViewData["Title"] = "S?n ph?m theo lo?i";

            // L?y t?t c? s?n ph?m thu?c lo?i ???c ch? ??nh
            var products = await _productRepository.GetAllAsync();
            products = products.Where(p => p.CategoryId == categoryId);

            int pageSize = 4;
            int pageNumber = page ?? 1;

            return View("Index", await products.ToPagedListAsync(pageNumber, pageSize));
        }

    }
}
