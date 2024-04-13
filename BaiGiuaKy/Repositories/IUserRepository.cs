using BaiGiuaKy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaiGiuaKy.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<List<ApplicationUser>> SearchAsync(string searchString);
    }
}
