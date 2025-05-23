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
    public class GroupController : ControllerBase
    {
        public readonly IGroupService _groupService;
        public readonly IUserService _userService;

        public GroupController(IGroupService groupService, IUserService userService)
        {
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }   

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

        //[host]/api/group/delete/
        [HttpDelete(nameof(Delete))]
        public async Task<string> Delete([FromBody] GroupDeleteDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            _userService.ParseUserId(userIdString);
            var result = await _groupService.DeleteGroupAsync(dto);
            return result;
        }

        [HttpPost("{groupIdString}/join")]
        public async Task<ActionResult<GroupMembershipFto>> JoinGroup(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest("Invalid GUID");
            
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            Guid userIdGuid = _userService.ParseUserId(userIdString);

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
                Console.WriteLine($"[{nameof(JoinGroup)}] An error occurred while joining the group.\n\t->{ex}");
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
                return BadRequest("Guid inválido!");

            var group = await _groupService.GetDisplayGroupByIdAsync(groupId);

            if (group == null)
                return NotFound("Grupo não encontrado!");
            return group;
        }
    }
}