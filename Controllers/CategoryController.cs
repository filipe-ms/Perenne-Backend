using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetById(Guid id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create(Category category)
        {
            var createdCategory = await _service.CreateAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpGet("{id}/groups")]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroupsByCategory(Guid id)
        {
            var groups = await _service.GetGroupsByCategoryAsync(id);
            return Ok(groups);
        }
    }
}