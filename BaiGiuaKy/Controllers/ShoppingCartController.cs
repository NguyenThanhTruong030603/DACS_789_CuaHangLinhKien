﻿using BaiGiuaKy.Extensions;
using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using BaiGiuaKy.Models.MoMo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaiGiuaKy.Service;
using static BaiGiuaKy.Models.Order;
using BaiGiuaky.Service.Vnpay;
using BaiGiuaky.Models.Vnpay;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BaiGiuaKy.Controllers
{
    [Authorize]
    [Authorize(Roles = SD.Role_Customer)]
    public class ShoppingCartController : Controller
    {

        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly IVnPayService _vnPayservice;
        private readonly IEmailSender _emailSender;
        private IMomoService _momoService;
        public ShoppingCartController(IEmailSender emailSender, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository, IMomoService momoService, IVnPayService vnPayservice)
        {
            _emailSender = emailSender;
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
			_vnPayservice = vnPayservice;
			_momoService = momoService;
        }
        


        public IActionResult ApplyDiscount(string discountCode)
		{
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
			if (cart == null || !cart.Items.Any())
			{
				TempData["Error"] = "Your cart is empty. Please add items to your cart before applying a discount.";
				return RedirectToAction("Index");
			}

			var discount = _context.Discounts.FirstOrDefault(d => d.Code == discountCode && d.ExpirationDate >= DateTime.Now);
			if (discount == null)
			{
				TempData["Error"] = "Invalid or expired discount code.";
				return RedirectToAction("Index");
			}

			cart.DiscountCode = discountCode;
			cart.DiscountPercentage = discount.Percentage;

			HttpContext.Session.SetObjectAsJson("Cart", cart);
			TempData["SuccessMessage"] = "Discount applied successfully!";
			return RedirectToAction("Index");
		}

        [HttpPost]
        public IActionResult AddSelectedProducts(List<int> selectedProducts)
        {
            // Kiểm tra nếu không có sản phẩm được chọn
            if (selectedProducts == null || !selectedProducts.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy giỏ hàng hiện tại từ Session hoặc tạo mới nếu chưa có
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            // Thêm từng sản phẩm đã chọn vào giỏ hàng
            foreach (var productId in selectedProducts)
            {
                var product = _productRepository.GetByIdAsync(productId).Result;
                if (product != null)
                {
                    var cartItem = new CartItem
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = 1 // Mặc định số lượng là 1
                    };

                    // Giả sử có phương thức AddItem để thêm sản phẩm vào giỏ
                    cart.AddItem(cartItem);
                }
            }

            // Lưu giỏ hàng vào Session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Thông báo thành công
            TempData["SuccessMessage"] = "Các sản phẩm đã được thêm vào giỏ hàng.";

            // Redirect đến trang giỏ hàng hoặc trang sản phẩm
            return RedirectToAction("Index", "ShoppingCart");
        }


        public IActionResult Checkout()
        {
          
            
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            if (cart == null || !cart.Items.Any())
			{
				TempData["Error"] = "Your cart is empty. Please add items to your cart before checking out.";
				return RedirectToAction("Index");
			}

			var order = new Order
			{
				TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity)
			};

			if (!string.IsNullOrEmpty(cart.DiscountCode))
			{
				order.TotalPrice = order.TotalPrice * (1 - cart.DiscountPercentage / 100);
			}

			return View(order);
		}
		[HttpPost]
		public async Task<IActionResult> Checkout(Order order)
		{
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
			var user = await _userManager.GetUserAsync(User);

			if (cart == null || !cart.Items.Any())
			{
				TempData["Error"] = "Your cart is empty. Please add items to your cart before checking out.";
				return RedirectToAction("Index");
			}

			HttpContext.Session.SetString("ShippingAddress", order.ShippingAddress);
			HttpContext.Session.SetString("Notes", order.Notes);
			HttpContext.Session.SetString("PaymentMethod", order.PaymentMethod);

			order.UserId = user.Id;
			order.OrderDate = DateTime.UtcNow;
			order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.DiscountPercentage = 0;
			
			if (!string.IsNullOrEmpty(cart.DiscountCode))
			{
				order.TotalPrice = order.TotalPrice * (1 - cart.DiscountPercentage / 100);
				order.DiscountCode = cart.DiscountCode;
                order.DiscountPercentage = cart.DiscountPercentage;
			}

			order.OrderDetails = cart.Items.Select(i => new OrderDetail
			{
				ProductId = i.ProductId,
				Quantity = i.Quantity,
				Price = i.Price
			}).ToList();

            if (order.PaymentMethod == "MoMo")
            {
                string id = Guid.NewGuid().ToString();
                var orderInfo = new OrderInfoModel
                {
                    Amount = (int)cart.Items.Sum(i => i.Price * i.Quantity * (1 - cart.DiscountPercentage / 100)),
                    FullName = user.UserName,
                    OrderId = id,
                    OrderInfo = "thanh toan cho don hang " + id
                };

                var response = await _momoService.CreatePaymentAsync(orderInfo);

                if (response == null || string.IsNullOrEmpty(response.PayUrl))
                {
                    TempData["Error"] = "There was an error creating the payment. Please try again later.";
                    return RedirectToAction("Index");
                }

                _context.Orders.Add(order);
                return Redirect(response.PayUrl);
            } else if (order.PaymentMethod == "VnPay")
            {


				// Create the VNPAY payment URL
				var vnPayModel = new VnPaymentRequestModel
				{
					Amount = (int)cart.Items.Sum(i => i.Price * i.Quantity * (1 - cart.DiscountPercentage / 100)),
					CreatedDate = DateTime.Now,
					Description = $"Thanh toan",
					FullName = user.UserName,
					OrderId = new Random().Next(1000, 100000)
				};


				
               // return View("DebugPayment", paymentInfo);
                _context.Orders.Add(order);
				// Redirect to VNPAY
				return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
			}

            else
            {
				order.Status = OrderStatus.ChờXácNhận;
				_context.Orders.Add(order);
				await _context.SaveChangesAsync();
				HttpContext.Session.Remove("Cart");
              
                return View("OrderCompleted", order.Id);
			}
		}

        

        public IActionResult AddToCart(int productId, int quantity)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                // Xử lý khi không tìm thấy sản phẩm
                return RedirectToAction("Index", "Home");
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };
            cart.AddItem(cartItem);
            
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["SuccessMessage"] = "Đã thêm vào giỏ hàng thành công!";
            return Redirect(Request.Headers["Referer"].ToString());


        }
        

        public IActionResult Index()
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
            ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            return View(cart);
        }
        // Các actions khác...
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
        public IActionResult RemoveFromCart(int productId)
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> OrderList()
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
            ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            var user = await _userManager.GetUserAsync(User);
            var orders = _context.Orders
                                 .Where(o => o.UserId == user.Id)
                                 .ToList();

            if (!orders.Any())
            {
                // Xử lý khi không có đơn hàng nào
                ViewBag.Message = "Bạn chưa có đơn hàng nào.";
                return View();
            }

            // Tạo ViewModel nếu muốn truyền thêm dữ liệu hoặc thông tin lý giải cho View
            return View(orders); // Gởi danh sách đơn hàng đến View
        }

        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
            ShoppingCart();
            ViewBag.CartItemCount = cart.Items.Sum(item => item.Quantity);
            var user = await _userManager.GetUserAsync(User);
            var order = _context.Orders
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.Product)
                                .FirstOrDefault(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                // Xử lý trường hợp không tìm thấy đơn hàng
                return RedirectToAction("OrderList");
            }

            // Trả về View với model là order đã tìm được
            return View(order);
        }
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            //Cập nhật số lượng của sản phẩm trong giỏ hàng



            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }

            return Ok(); 
        }
		
        [HttpGet]
        public async Task<IActionResult> PaymentCallBackVnPay()
        {
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            var user = await _userManager.GetUserAsync(User);
            Order order = new Order();
            string shippingAddress = HttpContext.Session.GetString("ShippingAddress");
            string notes = HttpContext.Session.GetString("Notes");
            string paymentMethod = HttpContext.Session.GetString("PaymentMethod");
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.ShippingAddress = shippingAddress;
            order.Notes = notes;
            order.PaymentMethod = paymentMethod;
            order.DiscountPercentage = 0;

            order.Status = OrderStatus.ChờXácNhận;
            if (!string.IsNullOrEmpty(cart.DiscountCode))
            {
                order.TotalPrice = order.TotalPrice * (1 - cart.DiscountPercentage / 100) ;
                order.DiscountCode = cart.DiscountCode;
                order.DiscountPercentage = cart.DiscountPercentage;
            }
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

			// Process the payment response from VNPAY
			var response = _vnPayservice.PaymentExecute(Request.Query);

			// Handle the response
			if (response.VnPayResponseCode == "00")
            {
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                return View("OrderCompleted", order.Id);
            }
            else
            {

                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["Message"] = "Giỏ hàng đã được xóa!";
            return RedirectToAction("Index"); // Quay lại trang giỏ hàng
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBackAsync()
        {
           
            var cart =
            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var user = await _userManager.GetUserAsync(User);
			Order order = new Order();
            string shippingAddress = HttpContext.Session.GetString("ShippingAddress");
            string notes = HttpContext.Session.GetString("Notes");
            string paymentMethod = HttpContext.Session.GetString("PaymentMethod");
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.ShippingAddress = shippingAddress;
            order.Notes = notes;
            order.PaymentMethod = paymentMethod;
            order.DiscountPercentage = 0;

			order.Status = OrderStatus.ChờXácNhận;
			if (!string.IsNullOrEmpty(cart.DiscountCode))
            {
                order.TotalPrice = order.TotalPrice * (1 - cart.DiscountPercentage / 100);
                order.DiscountCode = cart.DiscountCode;
                order.DiscountPercentage = cart.DiscountPercentage;
            }
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
               Price = i.Price
                }).ToList();
            if (response.IsSuccess == true)
            {
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                return View("OrderCompleted", order.Id);
            }
            else
            {
                return RedirectToAction("Index");
            }




        }


    }
}