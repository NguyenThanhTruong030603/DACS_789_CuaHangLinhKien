using BaiGiuaKy.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BaiGiuaKy.Models
{
    public enum OrderStatus
    {
        ChờXácNhận,
        ĐãXácNhận,
        ĐãGiao,
        ĐãGiaoThànhCông,
        ĐãHủy
    }
    public class Order
    {
		
		public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string? DiscountCode { get; set; }

        public decimal? DiscountPercentage { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập ghi chú")]
        public string Notes { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
		public OrderStatus Status { get; set; }
	}
}