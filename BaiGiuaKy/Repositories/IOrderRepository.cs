using BaiGiuaKy.Models;
using System.Threading.Tasks;

namespace BaiGiuaKy.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();

        Task<Order> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}
