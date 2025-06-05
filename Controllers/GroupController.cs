using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;
using System.Security.Claims;

namespace perenne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController(IGroupService groupService, IUserService userService) : ControllerBase
    {
        // [host]/api/group/{groupId}/
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupFTO>> GetGroupById(string id)
        {
            if (!Guid.TryParse(id, out Guid groupId)) return BadRequest("Guid inválido!");

            var group = await groupService.GetGroupByIdAsync(groupId);

            if (group == null) return NotFound("Grupo não encontrado!");

            var members = group.Members.Select(gm => new MemberFTO(gm)).ToList();

            var groupFto = new GroupFTO(group.Name, group.Description ?? string.Empty, members);

            return groupFto;
        }

        // [host]/api/{groupIdString}/join
        [HttpPost("{groupIdString}/join")]
        public async Task<ActionResult> JoinGroup(string groupIdString, [FromBody] JoinGroupRequestDto? dto) // Optional DTO for message
        {
            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest("Invalid Group GUID");

            var userId = GetCurrentUserId();
            var group = await groupService.GetGroupByIdAsync(groupId);
            if (group == null) return NotFound("Group not found.");

            if (group.IsPrivate)
            {
                var request = await groupService.RequestToJoinGroupAsync(userId, groupId, dto?.Message);
                return Ok(new { Message = "Solicitação para entrar no grupo enviada. Aguardando aprovação.", RequestId = request.Id });
            }
            else
            {
                var user = await userService.GetUserByIdAsync(userId);
                if (user == null) return NotFound("User not found.");

                GroupMember newMember = new()
                {
                    UserId = userId,
                    User = user,
                    GroupId = groupId,
                    Group = group,
                    Role = GroupRole.Member,
                };

                var member = await groupService.AddGroupMemberAsync(newMember);
                var response = new GroupJoinedFTO(member);
                return Ok(response);
            }
        }

        // [host]/api/group/getmain
        [HttpGet(nameof(GetMain))]
        public async Task<ActionResult<GroupFTO>> GetMain()
        {
            var userId = GetCurrentUserId();
            var group = await groupService.GetMainGroupAsync();
            var members = group!.Members.Select(gm => new MemberFTO(gm)).ToList();
            var groupFto = new GroupFTO(group.Name, group.Description ?? string.Empty, members);
            return Ok(groupFto);
        }

        // [host]/api/group/getall
        [HttpGet(nameof(GetAll))]
        public async Task<IEnumerable<GroupListFTO>> GetAll() =>
            await groupService.GetAllAsync();

        // Utils
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return userService.ParseUserId(userIdString);
        }
    }
}