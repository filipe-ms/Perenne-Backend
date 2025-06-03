using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Services
{
    public class GroupService(
        IGroupRepository repository,
        IChatService chatService,
        IFeedService feedService) : IGroupService
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

    }
}
