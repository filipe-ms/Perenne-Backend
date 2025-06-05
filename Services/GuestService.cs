using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services
{
    public class GuestService(IUserRepository userRepository, IGroupService groupService) : IGuestService
    {
        public async Task<bool> CreateUserAsync(User user)
        {
            var geral = await groupService.GetMainGroupAsync();
            var newuser = await userRepository.CreateUserAsync(user);
            if (geral == null)
            {
                var geralDTO = new GroupCreateDTO()
                {
                    Name = "Geral",
                    Description = "Bem-vindos à nossa plataforma!"
                };
                geral = await groupService.CreateGroupAsync(geralDTO);
            }

            var gm = new GroupMember()
            {
                UserId = newuser.Id,
                User = newuser,
                Group = geral,
                GroupId = geral.Id,
                Role = GroupRole.Member
            };

            var addmember = await groupService.AddGroupMemberAsync(gm);

            return addmember != null;
        }
        public async Task<User> UserLoginAsync(string email, string password)
        {
            var user = await userRepository.GetUserByEmailAsync(email);
            if (user == null || user.Password != password)
                throw new Exception("Invalid Email or Password");
            return user;
        }
    }
}
