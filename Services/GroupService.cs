using perenne.DTOs;
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
        public async Task<Group> AddGroupMemberAsync(AddGroupMemberDto dto)
        {
            var group = await _repository.GetGroupByIdAsync(dto.GroupId);
            var user = await _userService.GetUserByIdAsync(dto.UserId);


            if (group == null)
                throw new KeyNotFoundException($"Group with ID {dto.GroupId} not found");
            if (user == null)
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found");

            GroupMember member = new()
            {
                Role = GroupRole.Member,
                IsBlocked = false,
                IsMutedInGroupChat = false,

                UserId = dto.UserId,
                GroupId = dto.GroupId,

                User = user,
                Group = group,

                CreatedAt = DateTime.UtcNow
            };

            return await _repository.AddGroupMemberAsync(member);
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
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
