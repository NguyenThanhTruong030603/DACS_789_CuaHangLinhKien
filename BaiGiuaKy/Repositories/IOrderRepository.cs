using BaiGiuaKy.Models;
using System.Threading.Tasks;

namespace BaiGiuaKy.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync2(OrderStatus status);
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task DeleteAsync(int id);
        Task<List<Order>> SearchAsync(string searchString);
        Task UpdateAsync(Order order);
    }
}
