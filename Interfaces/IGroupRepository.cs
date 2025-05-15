using perenne.Models;
using static GroupMember;

namespace perenne.Repositories
{
    public interface IGroupRepository
    {
        Task<Group?> GetByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetAllAsync();
        Task AddAsync(Group group);
        Task UpdateAsync(Group group);
        Task DeleteAsync(Guid id);

        Task AddMemberAsync(Guid groupId, GroupMember member);
        Task RemoveMemberAsync(Guid groupId, Guid userId);
        Task ChangeMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole);
        Task<Feed?> GetFeedAsync(Guid groupId);
        Task<ChatChannel?> GetChatChannelAsync(Guid groupId);
    }
}