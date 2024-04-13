using BaiGiuaKy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaiGiuaKy.Repositories
{
    public class EFUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public EFUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<ApplicationUser>> SearchAsync(string searchString)
        {
            return await _context.Users
                .Where(user => user.UserName.Contains(searchString))
                .ToListAsync();
        }
    }
}
