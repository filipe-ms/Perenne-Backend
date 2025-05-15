using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Models;
using static GroupMember;

namespace perenne.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Group?> GetByIdAsync(Guid id)
        {
            return await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Feed)
                .Include(g => g.ChatChannel)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Feed)
                .Include(g => g.ChatChannel)
                .ToListAsync();
        }

        public async Task AddAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var group = await GetByIdAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddMemberAsync(Guid groupId, GroupMember member)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group != null)
            {
                group.Members.Add(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveMemberAsync(Guid groupId, Guid userId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group != null)
            {
                var member = group.Members.FirstOrDefault(m => m.UserId == userId);
                if (member != null)
                {
                    group.Members.Remove(member);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task ChangeMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group != null)
            {
                var member = group.Members.FirstOrDefault(m => m.UserId == userId);
                if (member != null)
                {
                    member.Role = newRole;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<Feed?> GetFeedAsync(Guid groupId)
        {
            return await _context.Feeds
                .FirstOrDefaultAsync(f => f.GroupId == groupId);
        }

        public async Task<ChatChannel?> GetChatChannelAsync(Guid groupId)
        {
            return await _context.ChatChannels
                .FirstOrDefaultAsync(c => c.GroupId == groupId);
        }
    }
}