using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;
using System.Security.Claims;
using perenne.Extensions;

namespace perenne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemAdminController(IGroupService groupService, IUserService userService) : ControllerBase
    {
        // [host]/api/sysadmin/creategroup/
        [HttpPost(nameof(CreateGroup))]
        public async Task<ActionResult<GroupCreateDto>> CreateGroup([FromBody] GroupCreateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userIdGuid))
                return Unauthorized("User ID could not be determined or is invalid.");

            var ready = await groupService.CreateGroupAsync(dto);
            return Ok(ready);
        }

        //[host]/api/sysadmin/groups/delete/
        [HttpDelete("group/delete")]
        public async Task<bool> DeleteGroup([FromBody] GroupDeleteDto dto)
        {
            var groupId = groupService.ParseGroupId(dto.GroupId);
            return await groupService.DeleteGroupAsync(groupId);
        }

        // [host]/api/sysadmin/groupmember/role/update
        [HttpPatch("groupmember/role/update")]
        public async Task<ActionResult<bool>> UpdateGroupMemberRole([FromBody] MemberRoleDTO member)
        {
            if(member == null || string.IsNullOrEmpty(member.GroupIdString) || string.IsNullOrEmpty(member.UserIdString) || string.IsNullOrEmpty(member.NewRoleString))
                return BadRequest("Invalid member role update request.");
            if (!Guid.TryParse(member.GroupIdString, out var groupId) || !Guid.TryParse(member.UserIdString, out var userId))
                return BadRequest("Invalid GUID format for UserId or GroupId.");

            var newRole = EnumExtensions.FromDisplayName<GroupRole>(member.NewRoleString.ToLower());

            var response = await groupService.UpdateGroupMemberRoleAsync(userId, groupId, newRole);
            return response;
        }

        // [host]/api/sysadmin/user/role/update
        [HttpPatch("user/role/update")]
        public async Task<ActionResult<bool>> UpdateUserRoleInSystemAsync(SystemRoleDTO userRole)
        {
            var userId = userService.ParseUserId(userRole.UserIdString);
            var newRole = EnumExtensions.FromDisplayName<SystemRole>(userRole.NewRoleString.ToLower());
            var response = await userService.UpdateUserRoleInSystemAsync(userId, newRole);
            return response;
        }
    }
}
