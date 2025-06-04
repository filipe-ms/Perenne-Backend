using Microsoft.Extensions.Logging;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services
{
    public class GroupService(
        IGroupRepository repository,
        IChatService chatService,
        IFeedService feedService,
        IUserService userService) : IGroupService
    {
        // Group CRUD
        public async Task<IEnumerable<GroupListFto>> GetAllAsync()
        {
            return await repository.GetAllAsync();
        }
        public async Task<Group> GetGroupByIdAsync(Guid id)
        {
            var group = await repository.GetGroupByIdAsync(id);
            return group ?? throw new KeyNotFoundException($"Group with ID {id} not found");
        }
        public async Task<GroupCreateDto> CreateGroupAsync(GroupCreateDto dto)
        {
            Group group = new()
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            var newgroup = await repository.CreateGroupAsync(group);
            if (newgroup == null)
                throw new Exception("Failed to create the group in the repository.");


            ChatChannel chat = new()
            {
                CreatedAt = DateTime.UtcNow,
                GroupId = newgroup.Id
            };

            Feed feed = new()
            {
                CreatedAt = DateTime.UtcNow,
                GroupId = newgroup.Id
            };

            var createdChatChannel = await chatService.CreateChatChannelAsync(chat);
            var createdFeed = await feedService.CreateFeedAsync(feed);

            newgroup.ChatChannel = createdChatChannel;
            newgroup.Feed = createdFeed;

            await repository.UpdateGroupAsync(newgroup);
            GroupCreateDto groupCreateDto = new()
            {
                Name = newgroup.Name,
                Description = newgroup.Description!
            };

            return groupCreateDto;
        }
        public async Task<bool> DeleteGroupAsync(Guid groupId)
        {
            return await repository.DeleteGroupAsync(groupId); ;
        }
        public async Task<Group> UpdateGroupAsync(Group group)
        {
            var updatedGroup = await repository.UpdateGroupAsync(group);
            return updatedGroup ?? throw new Exception("Failed to update the group in the repository.");
        }

        // Member operations
        public async Task<GroupMember> AddGroupMemberAsync(GroupMember newMember)
        {
            var createdMember = await repository.AddGroupMemberAsync(newMember) ?? throw new Exception("Failed to add member to the group in the repository.");

            return createdMember;
        }
        public async Task<GroupMember> GetGroupMemberAsync(Guid userId, Guid groupId)
        {
            return await repository.GetGroupMemberAsync(userId, groupId)
                ?? throw new KeyNotFoundException($"Group member with User ID {userId} and Group ID {groupId} not found.");
        }
        public async Task<bool> UpdateGroupMemberRoleAsync(Guid userId, Guid groupId, GroupRole newRole)
        {
            return await repository.UpdateGroupMemberRoleAsync(userId, groupId, newRole);
        }

        // Utils
        public Guid ParseGroupId(string groupIdString)
        {
            if (string.IsNullOrEmpty(groupIdString))
                throw new ArgumentNullException($"[GroupService] O parâmetro 'groupIdString' está nulo ou vazio. Um identificador de usuário é obrigatório.");
            if (!Guid.TryParse(groupIdString, out var guid))
                throw new ArgumentException($"[GroupService] O valor fornecido não é um GUID válido.");
            return guid;
        }


        // Group Join Request Operations
        public async Task<GroupJoinRequest> RequestToJoinGroupAsync(Guid userId, Guid groupId, string? message)
        {
            var group = await GetGroupByIdAsync(groupId);
            if (group == null) throw new KeyNotFoundException($"Group with ID {groupId} not found.");

            var user = await userService.GetUserByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found.");


            // Check if user is already a member
            var existingMember = await repository.GetGroupMemberAsync(userId, groupId);
            if (existingMember != null)
            {
                throw new InvalidOperationException("User is already a member of this group.");
            }

            // Check if there's already a pending request
            var existingRequest = await repository.GetPendingJoinRequestAsync(userId, groupId);
            if (existingRequest != null)
            {
                throw new InvalidOperationException("User already has a pending request for this group.");
            }

            var request = new GroupJoinRequest
            {
                UserId = userId,
                User = user,
                GroupId = groupId,
                Group = group,
                RequestedAt = DateTime.UtcNow,
                Status = RequestStatus.Pending,
                Message = message
            };

            return await repository.CreateJoinRequestAsync(request);
        }

        public async Task<IEnumerable<GroupJoinRequest>> GetPendingRequestsForGroupAsync(Guid groupId, Guid adminUserId)
        {
            var group = await GetGroupByIdAsync(groupId);
            if (group == null) throw new KeyNotFoundException($"Group with ID {groupId} not found.");

            var adminUser = await userService.GetUserByIdAsync(adminUserId);
            if (adminUser == null) throw new KeyNotFoundException($"Admin user with ID {adminUserId} not found.");

            var isAdminOrCoordinator = adminUser.SystemRole == SystemRole.Admin || adminUser.SystemRole == SystemRole.SuperAdmin;
            if (!isAdminOrCoordinator)
            {
                var membership = await repository.GetGroupMemberAsync(adminUserId, groupId);
                if (membership == null || membership.Role != GroupRole.Coordinator)
                {
                    throw new UnauthorizedAccessException("User is not authorized to view pending requests for this group.");
                }
            }

            return await repository.GetPendingJoinRequestsForGroupAsync(groupId);
        }

        public async Task<IEnumerable<GroupJoinRequest>> GetPendingRequestsForUserAsync(Guid userId)
        {
            var requests = await repository.GetJoinRequestsForUserAsync(userId);
            return requests.Where(r => r.Status == RequestStatus.Pending);
        }

        public async Task<GroupMember?> ApproveJoinRequestAsync(Guid requestId, Guid adminUserId)
        {
            var request = await repository.GetJoinRequestByIdAsync(requestId);
            if (request == null) throw new KeyNotFoundException($"Join request with ID {requestId} not found.");
            if (request.Status != RequestStatus.Pending) throw new InvalidOperationException("Request is not pending.");

            // Permission check (similar to GetPendingRequestsForGroupAsync)
            var group = await GetGroupByIdAsync(request.GroupId); // Ensures group context
            var adminUser = await userService.GetUserByIdAsync(adminUserId);
            if (adminUser == null) throw new KeyNotFoundException($"Admin user with ID {adminUserId} not found.");


            var isAdminOrCoordinator = adminUser.SystemRole == SystemRole.Admin || adminUser.SystemRole == SystemRole.SuperAdmin;
            if (!isAdminOrCoordinator)
            {
                var membership = await repository.GetGroupMemberAsync(adminUserId, request.GroupId);
                if (membership == null || membership.Role != GroupRole.Coordinator)
                {
                    throw new UnauthorizedAccessException("User is not authorized to approve requests for this group.");
                }
            }

            // Add user to group members
            var newMember = new GroupMember
            {
                UserId = request.UserId,
                User = request.User,
                GroupId = request.GroupId,
                Group = request.Group,
                Role = GroupRole.Member,
            };
            var addedMember = await repository.AddGroupMemberAsync(newMember);

            // Update request status
            request.Status = RequestStatus.Approved;
            request.HandledByUserId = adminUserId;
            request.HandledByUser = adminUser;
            request.HandledAt = DateTime.UtcNow;
            await repository.UpdateJoinRequestAsync(request);

            return addedMember;
        }

        public async Task<bool> RejectJoinRequestAsync(Guid requestId, Guid adminUserId)
        {
            var request = await repository.GetJoinRequestByIdAsync(requestId);
            if (request == null) throw new KeyNotFoundException($"Join request with ID {requestId} not found.");
            if (request.Status != RequestStatus.Pending) throw new InvalidOperationException("Request is not pending.");

            // Permission check (similar to ApproveJoinRequestAsync)
            var group = await GetGroupByIdAsync(request.GroupId);
            var adminUser = await userService.GetUserByIdAsync(adminUserId);
            if (adminUser == null) throw new KeyNotFoundException($"Admin user with ID {adminUserId} not found.");

            var isAdminOrCoordinator = adminUser.SystemRole == SystemRole.Admin || adminUser.SystemRole == SystemRole.SuperAdmin;
            if (!isAdminOrCoordinator)
            {
                var membership = await repository.GetGroupMemberAsync(adminUserId, request.GroupId);
                if (membership == null || membership.Role != GroupRole.Coordinator)
                {
                    throw new UnauthorizedAccessException("User is not authorized to reject requests for this group.");
                }
            }

            request.Status = RequestStatus.Rejected;
            request.HandledByUserId = adminUserId;
            request.HandledByUser = adminUser;
            request.HandledAt = DateTime.UtcNow;
            await repository.UpdateJoinRequestAsync(request);

            return true;
        }


        public async Task<GroupJoinRequest?> GetJoinRequestByIdAsync(Guid requestId)
        {
            var request = await repository.GetJoinRequestByIdAsync(requestId);
            return request;
        }

        public async Task<DateTime> MuteUserInGroupAsync(GroupMember groupMemberToMute)
        {
            if (groupMemberToMute == null)
                throw new ArgumentNullException(nameof(groupMemberToMute), "Group member to mute cannot be null.");
            if (groupMemberToMute.MutedUntil.HasValue && groupMemberToMute.MutedUntil.Value > DateTime.UtcNow)
                throw new InvalidOperationException("User is already muted until " + groupMemberToMute.MutedUntil.Value);

            var muted = await repository.UpdateGroupMemberAsync(groupMemberToMute);

            return (DateTime)muted.MutedUntil!;
        }

        public async Task<bool> UnmuteUserInGroupAsync(GroupMember groupMemberToMute)
        {
            if (groupMemberToMute == null)
                throw new ArgumentNullException(nameof(groupMemberToMute), "Group member to mute cannot be null.");
            if (groupMemberToMute.MutedUntil.HasValue && groupMemberToMute.MutedUntil.Value > DateTime.UtcNow)
                throw new InvalidOperationException("User is already muted until " + groupMemberToMute.MutedUntil.Value);

            var unmuted = await repository.UpdateGroupMemberAsync(groupMemberToMute);

            return unmuted.MutedUntil == null;
        }

        public async Task<bool> RemoveMemberFromGroupAsync(Guid groupId, Guid userId)
        {
            if (groupId == Guid.Empty || userId == Guid.Empty)
                throw new ArgumentException("Group ID and User ID must be valid GUIDs.");

            return await repository.RemoveMemberAsync(groupId, userId);
        }
    }
}