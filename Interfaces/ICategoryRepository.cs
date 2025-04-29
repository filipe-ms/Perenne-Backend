using perenne.Models;

namespace perenne.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(Guid id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Guid id, Category category); // Atualizar vai ser necessário?
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Group>> GetGroupsByCategoryAsync(Guid categoryId);
    }
}