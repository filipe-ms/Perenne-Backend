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
        Task<GroupMember> UpdateGroupMemberAsync(GroupMember member);
        Task<bool> UpdateGroupMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole);
        Task<GroupMember> GetGroupMemberAsync(Guid userId, Guid groupId);

        // Group Join Request Operations
        Task<GroupJoinRequest> CreateJoinRequestAsync(GroupJoinRequest request);
        Task<GroupJoinRequest?> GetJoinRequestByIdAsync(Guid requestId);
        Task<GroupJoinRequest?> GetPendingJoinRequestAsync(Guid userId, Guid groupId);
        Task<IEnumerable<GroupJoinRequest>> GetPendingJoinRequestsForGroupAsync(Guid groupId);
        Task<IEnumerable<GroupJoinRequest>> GetJoinRequestsForUserAsync(Guid userId);
        Task<GroupJoinRequest> UpdateJoinRequestAsync(GroupJoinRequest request);
        Task<bool> DeleteJoinRequestAsync(Guid requestId);
    }
}