using System.ComponentModel.DataAnnotations;

namespace BaiGiuaKy.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Khóa ngoại liên kết với Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Khóa ngoại liên kết với ApplicationUser (người dùng Identity)
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
