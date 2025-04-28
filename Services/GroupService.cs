using perenne.Models;
using perenne.Repositories;

namespace perenne.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _repository;

        public GroupService(IGroupRepository repository)
        {
            _repository = repository;
        }

        public async Task<Group> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task CreateAsync(Group group)
        {
            await _repository.AddAsync(group);
        }

        public async Task UpdateAsync(Guid id, Group group)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("Group not found");

            existing.Name = group.Name;
            existing.Description = group.Description;
            existing.ChatChannelId = group.ChatChannelId;
            existing.FeedChannelId = group.FeedChannelId;

            await _repository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
