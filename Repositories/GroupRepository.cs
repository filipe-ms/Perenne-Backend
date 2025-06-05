using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;
using System.Globalization;
using System.Text;

namespace perenne.Repositories
{
    public class GroupRepository(ApplicationDbContext context) : IGroupRepository
    {
        // Group CRUD
        public async Task<IEnumerable<GroupListFTO>> GetAllAsync()
        {
            var groups = await context.Groups.ToListAsync();

            var filteredGroups = groups
                .Where(g => !RemoveAccents(g.Name).Equals("geral", StringComparison.OrdinalIgnoreCase))
                .Select(g => new GroupListFTO
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    IsPrivate = g.IsPrivate,
                });

            return filteredGroups;
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
            bool exists = await context.Groups
                .AnyAsync(g => g.Name.ToLower() == group.Name.ToLower());

            if (exists)
            {
                throw new InvalidOperationException($"Já existe um grupo com o nome '{group.Name}'.");
            }

            var entry = await context.Groups.AddAsync(group);
            Group g = entry.Entity;
            await context.SaveChangesAsync();
            return g;
        }
        public async Task<Group> UpdateGroupAsync(Group group)
        {
            var existingGroup = await context.Groups.FindAsync(group.Id)
                ?? throw new KeyNotFoundException($"Grupo com ID {group.Id} não encontrado.");

            bool isGeral = existingGroup.Name.Equals("Geral", StringComparison.OrdinalIgnoreCase);
            bool nameChanged = !group.Name.Equals(existingGroup.Name, StringComparison.Ordinal);

            if (isGeral && nameChanged) throw new InvalidOperationException("Não é permitido alterar o nome do grupo 'Geral'.");

            context.Entry(existingGroup).CurrentValues.SetValues(group);

            await context.SaveChangesAsync();
            return existingGroup;
        }
        public async Task<bool> DeleteGroupAsync(Guid groupId)
        {
            var group = await context.Groups.FindAsync(groupId)
                ?? throw new KeyNotFoundException($"Grupo com ID {groupId} não encontrado.");

            if (group.Name.Equals("Geral", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Não é possível excluir o grupo 'Geral'.");
            }

            context.Groups.Remove(group);
            await context.SaveChangesAsync();
            return true;
        }

        // GroupMember
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
                .FirstOrDefaultAsync(g => g.Id == groupId) ?? throw new KeyNotFoundException($"Grupo com ID {groupId} não encontrado.");

            if (group.Name.Equals("Geral", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Não é possível excluir alguém do grupo 'Geral'.");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);

            if (member != null)
            {
                group.Members.Remove(member);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
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

        // Group Join Request
        public async Task<GroupJoinRequest> CreateJoinRequestAsync(GroupJoinRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

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
            ArgumentNullException.ThrowIfNull(request);
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

        // Outros
        public async Task<Group?> GetMainGroupAsync()
        {
            var mainGroupName = "Geral";
            var mainGroup = await context.Groups
                .FirstOrDefaultAsync(g => EF.Functions.ILike(g.Name, mainGroupName));
            return mainGroup;
        }
        public static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}