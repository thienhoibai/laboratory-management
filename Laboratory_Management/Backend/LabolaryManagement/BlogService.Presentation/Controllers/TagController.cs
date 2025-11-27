using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlogService.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Nhãn bài viết")]
    public class TagController : ControllerBase
    {
        private readonly Application.Services.TagService _tagService;
        public TagController(Application.Services.TagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet("GetAll")]
        [Authorize(Policy = "perm:Tag.List")]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _tagService.GetAllTags();
            return Ok(tags);
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Policy = "perm:Tag.View")]
        public async Task<IActionResult> GetTagById(int id)
        {
            var tag = await _tagService.GetTagById(id);
            if (tag == null)
            {
                return NotFound();
            }
            return Ok(tag);
        }
        
        [HttpPost("Add")]
        [Authorize(Policy = "perm:Tag.Create")]
        public async Task<IActionResult> AddTag([FromBody] string tag)
        {
            var result = await _tagService.AddTag(tag);
            if (result == 1)
            {
                return Conflict("Tag already exists.");
            }
            else
            {
                return Ok("Tag added successfully.");
            }
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = "perm:Tag.Delete")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _tagService.GetTagById(id);
            if (tag == null)
            {
                return NotFound();
            }
            await _tagService.DeleteTag(id);
            return Ok("Tag deleted successfully.");
        }

        [HttpPut("Update/{id}")]
        [Authorize(Policy = "perm:Tag.Update")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] string tagName)
        {
            var tag = await _tagService.GetTagById(id);
            if (tag == null)
            {
                return NotFound("Tag not Found");
            }
            await _tagService.UpdateTag(id, tagName);
            return Ok("Tag updated successfully.");
        }
    }
}
