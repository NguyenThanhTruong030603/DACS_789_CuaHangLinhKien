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

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index( int? page)
        {
            ViewData["Title"] = "Trang Ch?";

            var products = await _productRepository.GetAllAsync();

            

            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(await products.ToPagedListAsync(pageNumber, pageSize));
        }

        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> SearchAutocomplete(string term)
        //{
        //    try
        //    {
        //        var products = await _productRepository.GetAllAsync();
        //        var suggestions = products
        //            .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
        //            .Select(p => p.Name)
        //            .Distinct()
        //            .Take(10) // Limit suggestions to improve performance
        //            .ToList();

        //        return Json(suggestions);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error
        //        Debug.WriteLine("Error retrieving autocomplete suggestions: " + ex.Message);
        //        return BadRequest("Error retrieving autocomplete suggestions.");
        //    }
        //}
        // Hi?n th? thông tin chi ti?t s?n ph?m
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
