using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Repositories
{
    public class GroupRepository(ApplicationDbContext context) : IGroupRepository
    {
        // Group CRUD
        public async Task<IEnumerable<GroupListFto>> GetAllAsync()
        {
            var groups = await context.Groups.ToListAsync();

            var result = groups.Select(g => new GroupListFto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description
            });

            return result;
        }
        public async Task<Group> GetGroupByIdAsync(Guid id)
        {
            var group = await context.Groups
                .Include(g => g.Members)
                .Include(g => g.Feed)
                .Include(g => g.ChatChannel)
                .FirstOrDefaultAsync(g => g.Id == id);

            return group ?? throw new NullReferenceException("Não há grupos com esse ID");
        }
        public async Task<Group> CreateGroupAsync(Group group)
        {
            var entry = await context.Groups.AddAsync(group);
            Group g = entry.Entity;
            await context.SaveChangesAsync();
            return g;
        }
        public async Task<Group> UpdateGroupAsync(Group group)
        {
            var g = context.Groups.Update(group);
            await context.SaveChangesAsync();
            return g.Entity;
        }
        public async Task<bool> DeleteGroupAsync(Guid groupId)
        {
            var group = context.Groups.Find(groupId) ?? throw new KeyNotFoundException($"Grupo com ID {groupId} não encontrado.");
            context.Groups.Remove(group);
            await context.SaveChangesAsync();
            return true;
        }


        // GroupMember Operations
        public async Task<GroupMember> AddGroupMemberAsync(GroupMember member)
        {
            ArgumentNullException.ThrowIfNull(member);

            // Check if group exists
            var groupExists = await context.Groups.AnyAsync(g => g.Id == member.GroupId);
            if (!groupExists)
            {
                throw new InvalidOperationException($"Group with ID {member.GroupId} not found.");
            }

            // Check if user exists
            var userExists = await context.Users.AnyAsync(u => u.Id == member.UserId);

            if (!userExists)
                throw new InvalidOperationException($"User with ID {member.UserId} not found.");

            // Check if member already exists
            var existingMember = await context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == member.GroupId && gm.UserId == member.UserId);
            if (existingMember != null)
            {
                throw new InvalidOperationException($"User {member.UserId} is already a member of group {member.GroupId}.");
            }

            if (member.User != null) context.Entry(member.User).State = EntityState.Unchanged;
            if (member.Group != null) context.Entry(member.Group).State = EntityState.Unchanged;

            await context.GroupMembers.AddAsync(member);
            await context.SaveChangesAsync();

            await context.Entry(member).Reference(m => m.User).LoadAsync();
            await context.Entry(member).Reference(m => m.Group).LoadAsync();


            return member; // Return the newly created GroupMember entity
        }
        public async Task<bool> RemoveMemberAsync(Guid groupId, Guid userId)
        {
            var group = await context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group != null)
            {
                var member = group.Members.FirstOrDefault(m => m.UserId == userId);
                if (member != null)
                {
                    group.Members.Remove(member);
                    await context.SaveChangesAsync();
                }
            }
            return true;
        }
        public async Task<bool> UpdateGroupMemberRoleAsync(Guid groupId, Guid userId, GroupRole newRole)
        {
            var group = await context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group != null)
            {
                var member = group.Members.FirstOrDefault(m => m.UserId == userId);
                if (member != null)
                {
                    member.Role = newRole;
                    await context.SaveChangesAsync();
                }
            }
            return true;
        }
        public async Task<GroupMember> GetGroupMemberAsync(Guid userId, Guid groupId)
        {
            var member = await context.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync(gm => gm.UserId == userId && gm.GroupId == groupId);
            return member ?? throw new KeyNotFoundException($"Member with User ID {userId} not found in Group ID {groupId}.");
        }
    }
}