using perenne.FTOs;
using perenne.Models;

namespace perenne.Interfaces
{
    public interface IGroupRepository
    {
        // Group CRUD
        Task<IEnumerable<GroupListFto>> GetAllAsync();
        Task<Group> GetGroupByIdAsync(Guid id);
        Task<Group> CreateGroupAsync(Group group);
        Task<bool> DeleteGroupAsync(Guid groupId);
        Task<Group> UpdateGroupAsync(Group group);

        // GroupMember Operations
        Task<GroupMember> AddGroupMemberAsync(GroupMember member);
        Task<bool> RemoveMemberAsync(Guid groupId, Guid userId);
        Task<bool> UpdateGroupMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole);
        Task<GroupMember> GetGroupMemberAsync(Guid userId, Guid groupId);
    }
}