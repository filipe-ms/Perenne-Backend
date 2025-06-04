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

        // Group Join Request Operations
        Task<GroupJoinRequest?> GetJoinRequestByIdAsync(Guid requestId);
        Task<GroupJoinRequest> RequestToJoinGroupAsync(Guid userId, Guid groupId, string? message);
        Task<IEnumerable<GroupJoinRequest>> GetPendingRequestsForGroupAsync(Guid groupId, Guid adminUserId);
        Task<IEnumerable<GroupJoinRequest>> GetPendingRequestsForUserAsync(Guid userId);
        Task<GroupMember?> ApproveJoinRequestAsync(Guid requestId, Guid adminUserId);
        Task<bool> RejectJoinRequestAsync(Guid requestId, Guid adminUserId);

        // Group operations
        Task<DateTime> MuteUserInGroupAsync(GroupMember groupMemberToMute);
        Task<bool> UnmuteUserInGroupAsync(GroupMember groupMemberToMute);
        Task<bool> RemoveMemberFromGroupAsync(Guid groupId, Guid userId);
    }
}