using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Services;
using System.Security.Claims;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController(IGroupService _groupService) : ControllerBase
    {
        [HttpPost(nameof(Create))]
        public async Task Create(
            [FromBody] GroupCreateDto dto) =>
            await _groupService.CreateGroupAsync(dto);

        [Authorize]
        [HttpPost("{groupId}/join")]
        public async Task<ActionResult<GroupMembershipFto>> JoinGroup(Guid groupId) // Changed return type
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userIdGuid))
            {
                return Unauthorized("User ID could not be determined or is invalid.");
            }

            try
            {
                var membershipDto = await _groupService.AddGroupMemberAsync(groupId, userIdGuid);
                return Ok(membershipDto);
            }
            catch (KeyNotFoundException knfex)
            {
                return NotFound(new { message = knfex.Message });
            }
            catch (InvalidOperationException ioex)
            {
                return Conflict(new { message = ioex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while joining the group.");
            }
        }

        [HttpGet(nameof(GetAll))]
        public async Task<IEnumerable<GroupListFto>> GetAll() =>
            await _groupService.GetAllAsync();


        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetById(Guid id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound();
            return Ok(group);
        }


        /*
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, Group group)
        {
            await _groupService.UpdateAsync(id, group);
            return NoContent();
        }*/

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _groupService.DeleteAsync(id);
            return NoContent();
        }
    }
}