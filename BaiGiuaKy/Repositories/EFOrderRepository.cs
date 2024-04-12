using Microsoft.EntityFrameworkCore;
using BaiGiuaKy.Models;

namespace BaiGiuaKy.Repositories
{
    
    public class EFOrderRepository : IOrderRepository
    {
        public readonly ApplicationDbContext _context;

        public EFOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
            .Include(p => p.OrderDetails) // Include thông tin về category
            .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id);
            // lấy thông tin kèm theo category
            return await _context.Orders.Include(p => p.OrderDetails).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var Order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(Order);
            await _context.SaveChangesAsync();
        }
    }
}
