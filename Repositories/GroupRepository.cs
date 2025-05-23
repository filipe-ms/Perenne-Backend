using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;

namespace perenne.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            var entry = await _context.Groups.AddAsync(group);
            Group g = entry.Entity;
            await _context.SaveChangesAsync();
            return g;
        }
        public async Task<string> DeleteGroupAsync(Guid groupId)
        {
            var group = _context.Groups.Find(groupId); 
            if (group == null) throw new KeyNotFoundException($"Grupo com ID {groupId} não encontrado.");
            var groupName = group.Name;
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return $"Grupo {groupName} removido com sucesso.";
        }
        public async Task<Group> UpdateGroupAsync(Group group)
        {
            var g = _context.Groups.Update(group);
            await _context.SaveChangesAsync();
            return g.Entity;
        }

        public async Task<GetGroupByIdFto> GetDisplayGroupByIdAsync(Guid id)
        
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) 
                throw new NullReferenceException("Não há grupos com esse ID");

            var GroupFto = new GetGroupByIdFto
            {
                Name = group.Name,
                Description = group.Description,
                MemberList = group.Members.Select(gm => new MemberFto
                {
                    UserId = gm.UserId,
                    FirstName = gm.User.FirstName,
                    LastName = gm.User.LastName,
                    RoleInGroup = gm.Role,
                    IsBlocked = gm.IsBlocked,
                    IsMutedInGroupChat = gm.IsMutedInGroupChat
                }).ToList()
            };

            if(GroupFto == null)
                throw new NullReferenceException("Não há grupos com esse ID");

            return GroupFto;
        }


        public async Task<Group> GetGroupByIdAsync(Guid id)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Feed)
                .Include(g => g.ChatChannel)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
                throw new NullReferenceException("Não há grupos com esse ID");

            return group;
        }

        public async Task<IEnumerable<GroupListFto>> GetAllAsync()
        {
            var groups = await _context.Groups.ToListAsync();

            var result = groups.Select(g => new GroupListFto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description
            });

            return result;
        }

        
        public async Task DeleteAsync(Guid id)
        {
            var group = await GetGroupByIdAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
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

        // Group Member Operations
        public async Task<GroupMember> AddGroupMemberAsync(GroupMember member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            // Check if group exists
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == member.GroupId);
            if (!groupExists)
            {
                throw new InvalidOperationException($"Group with ID {member.GroupId} not found.");
            }

            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == member.UserId);
            
            if (!userExists)
                throw new InvalidOperationException($"User with ID {member.UserId} not found.");

            // Check if member already exists
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == member.GroupId && gm.UserId == member.UserId);
            if (existingMember != null)
            {
                throw new InvalidOperationException($"User {member.UserId} is already a member of group {member.GroupId}.");
            }

            if (member.User != null) _context.Entry(member.User).State = EntityState.Unchanged;
            if (member.Group != null) _context.Entry(member.Group).State = EntityState.Unchanged;

            await _context.GroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();

            await _context.Entry(member).Reference(m => m.User).LoadAsync();
            await _context.Entry(member).Reference(m => m.Group).LoadAsync();


            return member; // Return the newly created GroupMember entity
        }
    }
}