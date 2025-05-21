using perenne.DTOs;
using perenne.FTOs;

namespace perenne.Interfaces
{
    public interface IGroupService
    {
        Task<Group> CreateGroupAsync(GroupCreateDto dto);
        Task<Group?> GetGroupByIdAsync(Guid id);
        Task<GroupMembershipFto> AddGroupMemberAsync(Guid groupId, Guid userId); 
        Task<IEnumerable<GroupListFto>> GetAllAsync();
        Task DeleteAsync(Guid id);
    }
}