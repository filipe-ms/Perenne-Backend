using perenne.DTOs;
using perenne.FTOs;

namespace perenne.Interfaces
{
    public interface IGroupService
    {
        Task<GroupCreateDto> CreateGroupAsync(GroupCreateDto dto);
        Task<Group> GetGroupByIdAsync(Guid id);
        Task<GetGroupByIdFto> GetDisplayGroupByIdAsync(Guid id);
        Task<GroupMembershipFto> AddGroupMemberAsync(Guid groupId, Guid userIdToAdd);
        Task<IEnumerable<GroupListFto>> GetAllAsync();
        Task<string> DeleteGroupAsync(GroupDeleteDto id);
    }
}