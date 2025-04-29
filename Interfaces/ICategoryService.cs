using perenne.Models;

namespace perenne.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(Guid id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Guid id, Category category);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Group>> GetGroupsByCategoryAsync(Guid categoryId);
    }
}