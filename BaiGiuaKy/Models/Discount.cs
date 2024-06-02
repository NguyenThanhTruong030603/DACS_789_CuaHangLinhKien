namespace BaiGiuaKy.Models
{
    public class Discount
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public decimal Percentage { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
