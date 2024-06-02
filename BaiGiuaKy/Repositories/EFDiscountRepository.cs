using BaiGiuaKy.Models;
using BaiGiuaKy.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BaiGiuaKy.Repositories
{
    namespace BaiGiuaKy.Repositories
    {
        public class EFDiscountRepository : IDiscountRepository
        {
            private readonly ApplicationDbContext _context;

            public EFDiscountRepository(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Discount>> GetAllAsync()
            {
                return await _context.Discounts.ToListAsync();


            }

            public async Task<Discount> GetByIdAsync(int id)
            {
                return await _context.Discounts.FindAsync(id);

            }

            public async Task AddAsync(Discount discount)
            {
                _context.Discounts.Add(discount);
                await _context.SaveChangesAsync();
            }

            public async Task UpdateAsync(Discount discount)
            {
                _context.Discounts.Update(discount);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(int id)
            {
                var discount = await _context.Discounts.FindAsync(id);
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }

            public async Task<List<Discount>> SearchAsync(string searchString)
            {
                return await _context.Discounts.Where(p => p.Code.Contains(searchString)).ToListAsync();
            }

        }
    }
}
