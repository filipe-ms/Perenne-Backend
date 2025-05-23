using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;
// Assuming perenne.Repositories is where IGroupRepository is defined
// using perenne.Repositories; 

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

        // Modified to accept creatorUserId for auditing
        public async Task<GroupCreateDto> CreateGroupAsync(GroupCreateDto dto)
        {
            Group group = new()
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            var newgroup = await _repository.CreateGroupAsync(group);
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

            var createdChatChannel = await _chatService.CreateChatChannelAsync(chat);
            var createdFeed = await _feedService.CreateFeedAsync(feed);

            newgroup.ChatChannel = createdChatChannel;
            newgroup.Feed = createdFeed;
            
            await _repository.UpdateGroupAsync(newgroup);
            GroupCreateDto groupCreateDto = new()
            {
                Name = newgroup.Name,
                Description = newgroup.Description!
            };

            return groupCreateDto;
        }
        public async Task<string> DeleteGroupAsync(GroupDeleteDto dto)
        {
            Guid groupId = dto.GroupId;
            string name = await _repository.DeleteGroupAsync(groupId);
            if(string.IsNullOrEmpty(name)) throw new KeyNotFoundException($"Group with ID {groupId} not found");
            return await _repository.DeleteGroupAsync(groupId); ;
        }
        public async Task<Group> GetGroupByIdAsync(Guid id)
        {
            var group = await _repository.GetGroupByIdAsync(id);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {id} not found");
            return group;
        }
        public async Task<GetGroupByIdFto> GetDisplayGroupByIdAsync(Guid id)
        {
            var group = await _repository.GetDisplayGroupByIdAsync(id);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {id} not found");
            return group;
        }
        public async Task<GroupMembershipFto> AddGroupMemberAsync(Guid groupId, Guid userIdToAdd)
        {
            var userToAdd = await _userService.GetUserByIdAsync(userIdToAdd);
            if (userToAdd == null)
                throw new KeyNotFoundException($"User to add with ID {userIdToAdd} not found.");

            var group = await _repository.GetGroupByIdAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException($"Group with ID {groupId} not found.");

            GroupMember newMember = new()
            {
                Role = GroupRole.Member,
                IsBlocked = false,
                IsMutedInGroupChat = false,
                UserId = userIdToAdd,
                GroupId = groupId,
                CreatedAt = DateTime.UtcNow,
                User = await _userService.GetUserByIdAsync(userIdToAdd),
                Group = await GetGroupByIdAsync(groupId)
            };

            var createdMember = await _repository.AddGroupMemberAsync(newMember);
            if (createdMember == null)
                throw new Exception("Failed to add member to the group in the repository.");
            

            return new GroupMembershipFto
            {
                GroupId = createdMember.GroupId,
                GroupName = createdMember.Group?.Name ?? group.Name, // Fallback if not loaded
                UserId = createdMember.UserId,
                UserFirstName = createdMember.User?.FirstName ?? userToAdd.FirstName, // Fallback
                UserLastName = createdMember.User?.LastName ?? userToAdd.LastName,   // Fallback
                RoleInGroup = createdMember.Role,
                JoinedAt = createdMember.CreatedAt,
                Message = $"User {userToAdd.FirstName} successfully joined group {group.Name}."
            };
        }
        public async Task<IEnumerable<GroupListFto>> GetAllAsync() =>
            await _repository.GetAllAsync();
    }
}
