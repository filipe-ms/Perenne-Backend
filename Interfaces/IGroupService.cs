using perenne.DTOs;
using perenne.FTOs;
using perenne.Models;

namespace perenne.Interfaces
{
    public interface IGroupService
    {
        // Group CRUD
        Task<IEnumerable<GroupListFto>> GetAllAsync();
        Task<Group> GetGroupByIdAsync(Guid id);
        Task<GroupCreateDto> CreateGroupAsync(GroupCreateDto dto);
        Task<bool> DeleteGroupAsync(Guid groupId);
        Task<Group> UpdateGroupAsync(Group group);

        // Member operations
        Task<GroupMember> AddGroupMemberAsync(GroupMember newMember);
        Task<GroupMember> GetGroupMemberAsync(Guid userId, Guid groupId);
        Task<bool> UpdateGroupMemberRoleAsync(Guid userId, Guid groupId, GroupRole newRole);

        // Utils
        Guid ParseGroupId(string groupIdString);
    }
}