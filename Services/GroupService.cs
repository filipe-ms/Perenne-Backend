using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _repository;
        private readonly IChatService _chatService;
        private readonly IFeedService _feedService;
        private readonly IUserService _userService;

        public GroupService(
            IGroupRepository repository,
            IChatService chatService,
            IFeedService feedService,
            IUserService userService
            )
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _feedService = feedService ?? throw new ArgumentNullException(nameof(feedService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Group> CreateGroupAsync(GroupCreateDto dto)
        {
            Group group = new()
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            if (group == null) throw new ArgumentNullException(nameof(group));

            Group newgroup = await _repository.AddAsync(group);

            ChatChannel chat = new()
            {
                CreatedAt = DateTime.UtcNow,
                GroupId = newgroup.Id,
                Group = newgroup
            };

            Feed feed = new()
            {
                CreatedAt = DateTime.UtcNow,
                GroupId = newgroup.Id,
                Group = newgroup
            };

            await _chatService.CreateChatChannelAsync(chat);
            await _feedService.CreateFeedAsync(feed);

            newgroup.ChatChannel = chat;
            newgroup.Feed = feed;

            await _repository.UpdateGroupAsync(newgroup);

            return newgroup;
        }
        public async Task<Group> GetGroupByIdAsync(Guid id)
        {
            var group = await _repository.GetGroupByIdAsync(id);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {id} not found");
            return group;
        }

        public async Task<GroupMembershipFto> AddGroupMemberAsync(Guid groupId, Guid userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var group = await _repository.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                throw new KeyNotFoundException($"Group with ID {groupId} not found.");
            }

            GroupMember newMember = new()
            {
                Role = GroupRole.Member,
                IsBlocked = false,
                IsMutedInGroupChat = false,
                UserId = userId,
                GroupId = groupId,
                CreatedAt = DateTime.UtcNow,
                User = await _userService.GetUserByIdAsync(userId),
                Group = await GetGroupByIdAsync(groupId),
            };

            var createdMember = await _repository.AddGroupMemberAsync(newMember);

            return new GroupMembershipFto
            {
                GroupId = createdMember.GroupId,
                GroupName = createdMember.Group.Name,
                UserId = createdMember.UserId,
                UserFirstName = createdMember.User.FirstName,
                UserLastName = createdMember.User.LastName,
                RoleInGroup = createdMember.Role,
                JoinedAt = createdMember.CreatedAt,
                Message = $"User {user.FirstName} successfully joined group {group.Name}."
            };
        }

        public async Task<IEnumerable<GroupListFto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            // you might want to check for existence first, or let the repo throw
            await _repository.DeleteAsync(id);
        }
    }
}
