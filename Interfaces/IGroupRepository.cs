using perenne.Models;
using static GroupMember;

namespace perenne.Interfaces
{
    public interface IGroupRepository
    {
        Task<Group?> GetGroupByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group> AddAsync(Group group);
        Task<Group> UpdateGroupAsync(Group group);
        Task DeleteAsync(Guid id);
        Task<Group> AddGroupMemberAsync(GroupMember member);
        Task RemoveMemberAsync(Guid groupId, Guid userId);
        Task ChangeMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole);
    }
}