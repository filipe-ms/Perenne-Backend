using Microsoft.AspNetCore.Mvc;
using perenne.Models;
using perenne.Services;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/groups/add")]
    public class GroupController(IGroupService service) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Group>>> GetAll()
        {
            var groups = await service.GetAllAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Group>> GetById(Guid id)
        {
            var group = await service.GetByIdAsync(id);
            if (group == null)
                return NotFound();
            return Ok(group);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Group group)
        {
            await service.CreateAsync(group);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, Group group)
        {
            await service.UpdateAsync(id, group);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await service.DeleteAsync(id);
            return NoContent();
        }
    }
}