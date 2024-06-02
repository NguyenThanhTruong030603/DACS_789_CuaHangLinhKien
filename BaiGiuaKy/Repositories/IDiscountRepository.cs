using BaiGiuaKy.Models;


namespace BaiGiuaKy.Repositories
{
    public interface IDiscountRepository
    {
        Task<IEnumerable<Discount>> GetAllAsync();
        Task<Discount> GetByIdAsync(int id);
        Task AddAsync(Discount discount);
        Task UpdateAsync(Discount discount);
        Task DeleteAsync(int id);
        Task<List<Discount>> SearchAsync(string searchString);
    }
}
