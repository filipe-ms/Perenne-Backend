using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController(IGroupService groupService) : ControllerBase
    {
        [HttpPost(nameof(Create))]
        public async Task Create(
            [FromBody] GroupCreateDto dto) =>
            await groupService.CreateGroupAsync(dto);

        [HttpPost("{groupId}/AddMember")]
        public async Task<Group> AddMember(Guid groupId, [FromBody] AddGroupMemberDto dto)
        {
            dto.GroupId = groupId;
            return await groupService.AddGroupMemberAsync(dto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetAll()
        {
            var groups = await groupService.GetAllAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetById(Guid id)
        {
            var group = await groupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound();
            return Ok(group);
        }

        /*
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, Group group)
        {
            await groupService.UpdateAsync(id, group);
            return NoContent();
        }*/

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await groupService.DeleteAsync(id);
            return NoContent();
        }
    }
}