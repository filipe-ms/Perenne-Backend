using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var members = group.Members.Select(gm => new MemberFto(gm)).ToList();

            var groupFto = new GroupFTO(group.Name, group.Description ?? string.Empty, members);

            return groupFto;
        }

        // [host]/api/{groupIdString}/join
        [HttpPost("{groupIdString}/join")]
        public async Task<ActionResult<GroupJoinedFTO>> JoinGroup(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest("Invalid GUID");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid userId = userService.ParseUserId(userIdString);

            GroupMember newMember = new()
            {
                UserId = userId,
                GroupId = groupId,
                User = await userService.GetUserByIdAsync(userId),
                Group = await groupService.GetGroupByIdAsync(groupId)
            };

            var member = await groupService.AddGroupMemberAsync(newMember);

            if (member == null) return NotFound("Group not found or user already a member.");

            var response = new GroupJoinedFTO(member);

            return Ok(response);
        }

        // [host]/api/group/getall
        [HttpGet(nameof(GetAll))]
        public async Task<IEnumerable<GroupListFto>> GetAll() =>
            await groupService.GetAllAsync();
    }
}