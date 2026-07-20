using BlogService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogService.Application.DTOs;

namespace BlogService.Presentation.Controllers
{
    [Route("api/comments")]
    [ApiController]
    [Tags("Comments")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _service;

        public CommentController(CommentService service)
        {
            _service = service;
        }

        [HttpGet("/api/blog-posts/{postId}/comments")]
        public async Task<IActionResult> GetByPost(
            int postId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var allComments = await _service.GetByPostIdAsync(postId);
            var totalItems = allComments.Count;

            var data = allComments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _service.GetByIdAsync(id);
            return comment == null ? NotFound() : Ok(comment);
        }

        [HttpPost]
        [Authorize(Policy = "perm:Comment.Create")]
        public async Task<IActionResult> Create([FromBody] CommentDTO dto)
        {
            await _service.AddAsync(dto);
            return Ok("Comment created successfully.");
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "perm:Comment.Update")]
        public async Task<IActionResult> Update(int id, [FromBody] CommentDTO dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(new { message = "Comment updated successfully." });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:Comment.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Comment deleted successfully.");
        }

        [HttpGet]
        [Authorize(Policy = "perm:Comment.Search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _service.SearchAsync(keyword);
            return Ok(result);
        }
    }
}
