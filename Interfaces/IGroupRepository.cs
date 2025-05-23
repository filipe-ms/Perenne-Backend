using perenne.DTOs;
using perenne.FTOs;
using perenne.Models;
using static GroupMember;

namespace perenne.Interfaces
{
    public interface IGroupRepository
    {
        Task<Group> GetGroupByIdAsync(Guid id);
        Task<GetGroupByIdFto> GetDisplayGroupByIdAsync(Guid id);
        Task<IEnumerable<GroupListFto>> GetAllAsync();
        Task<Group> CreateGroupAsync(Group group);
        Task<Group> UpdateGroupAsync(Group group);
        Task DeleteAsync(Guid id);
        Task<GroupMember> AddGroupMemberAsync(GroupMember member);
        Task RemoveMemberAsync(Guid groupId, Guid userId);
        Task ChangeMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole);
        Task<string> DeleteGroupAsync(Guid groupId);
    }
}