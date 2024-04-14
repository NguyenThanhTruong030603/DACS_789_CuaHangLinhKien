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

            

            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(await products.ToPagedListAsync(pageNumber, pageSize));
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
            ViewData["Title"] = "T�m ki?m";

            // Retrieve the search string from TempData if available
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

    }
}
