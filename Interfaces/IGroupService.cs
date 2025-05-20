using perenne.DTOs;
using perenne.Models;

namespace perenne.Interfaces
{
    public interface IGroupService
    {
        Task<Group> CreateGroupAsync(GroupCreateDto dto);
        Task<Group> GetGroupByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetAllAsync();
        Task DeleteAsync(Guid id);

        // Group member ops
        Task<Group> AddGroupMemberAsync(AddGroupMemberDto dto);
    }
}