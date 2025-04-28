using perenne.Models;

namespace perenne.Services
{
    public interface IGroupService
    {
        Task<Group> GetByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetAllAsync();
        Task CreateAsync(Group group);
        Task UpdateAsync(Guid id, Group group);
        Task DeleteAsync(Guid id);
    }
}