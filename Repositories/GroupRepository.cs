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
                Description = g.Description,
                IsPrivate = g.IsPrivate,
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
        public async Task<GroupMember> UpdateGroupMemberAsync(GroupMember member)
        {
            var existing = await context.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync(gm => gm.UserId == member.UserId) ?? throw new KeyNotFoundException($"Membro ou Grupo com o ID fornecido não encontrado.");

            existing = member;
            await context.SaveChangesAsync();
            return existing;
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

        // Group Join Request Operations (New)
        public async Task<GroupJoinRequest> CreateJoinRequestAsync(GroupJoinRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Detach related entities if they are already tracked to avoid issues with AddAsync
            if (request.User != null) context.Entry(request.User).State = EntityState.Unchanged;
            if (request.Group != null) context.Entry(request.Group).State = EntityState.Unchanged;

            await context.GroupJoinRequests.AddAsync(request);
            await context.SaveChangesAsync();
            return request;
        }

        public async Task<GroupJoinRequest?> GetJoinRequestByIdAsync(Guid requestId)
        {
            return await context.GroupJoinRequests
                .Include(r => r.User)
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task<GroupJoinRequest?> GetPendingJoinRequestAsync(Guid userId, Guid groupId)
        {
            return await context.GroupJoinRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.GroupId == groupId && r.Status == RequestStatus.Pending);
        }

        public async Task<IEnumerable<GroupJoinRequest>> GetPendingJoinRequestsForGroupAsync(Guid groupId)
        {
            return await context.GroupJoinRequests
                .Where(r => r.GroupId == groupId && r.Status == RequestStatus.Pending)
                .Include(r => r.User) // Include user details for the admin to see
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupJoinRequest>> GetJoinRequestsForUserAsync(Guid userId)
        {
            return await context.GroupJoinRequests
                .Where(r => r.UserId == userId)
                .Include(r => r.Group) // Include group details
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<GroupJoinRequest> UpdateJoinRequestAsync(GroupJoinRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            context.GroupJoinRequests.Update(request);
            await context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> DeleteJoinRequestAsync(Guid requestId)
        {
            var request = await context.GroupJoinRequests.FindAsync(requestId);
            if (request != null)
            {
                context.GroupJoinRequests.Remove(request);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}