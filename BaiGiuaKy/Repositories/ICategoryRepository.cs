﻿using BaiGiuaKy.Models;

namespace BaiGiuaKy.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task<List<Category>> SearchAsync(string searchString);
    }

}
