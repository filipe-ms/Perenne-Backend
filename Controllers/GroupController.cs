using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using System.Security.Claims;

namespace perenne.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class GroupController(IGroupService _groupService) : ControllerBase
    {
        // [host]/api/group/create/
        [HttpPost(nameof(Create))]
        public async Task<ActionResult<GroupCreateDto>> Create([FromBody] GroupCreateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userIdGuid))
                return Unauthorized("User ID could not be determined or is invalid.");

            var ready = await _groupService.CreateGroupAsync(dto);
            return Ok(ready);
        }

        [HttpPost("{groupIdString}/join")]
        public async Task<ActionResult<GroupMembershipFto>> JoinGroup(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest("Invalid GUID");
            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userIdGuid))
                return Unauthorized("User ID could not be determined or is invalid.");

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

        // [host]/api/group/getall
        [HttpGet(nameof(GetAll))]
        public async Task<IEnumerable<GroupListFto>> GetAll() =>
            await _groupService.GetAllAsync();

        // [host]/api/group/{groupId}/
        [HttpGet("{id}")] 
        public async Task<ActionResult<GetGroupByIdFto>> GetGroupById(string id)
        {
            if (!Guid.TryParse(id, out Guid groupId))
                return BadRequest("Invalid GUID format");
            var group = await _groupService.GetDisplayGroupByIdAsync(groupId);
            if (group == null)
                return NotFound();
            return group;
        }

        // [host]/api/group/delete/{groupId}/
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _groupService.DeleteAsync(id);
            return NoContent();
        }
    }
}