using BlogService.Application.Services;
using BlogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlogService.Application.DTOs;
using BlogService.Application.Enums;

namespace BlogService.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Bài viết")]
    public class BlogPostController : ControllerBase
    {
        private readonly BlogPostService _service;

        public BlogPostController(BlogPostService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogs(
    [FromQuery] Guid? authorId,
    [FromQuery] int? status,
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var totalItems = await _service.GetAllWithCategoryAsync(authorId, status, 1, int.MaxValue,search)
                .ContinueWith(t => t.Result.Count);

            var data = await _service.GetAllWithCategoryAsync(authorId, status, page, pageSize, search);

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
            var post = await _service.GetByIdAsync(id);
            return post == null ? NotFound() : Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogPostCreateDTO dto)
        {
            await _service.AddAsync(dto);
            return Ok("Post created successfully.");
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostDTO dto)
        {

            await _service.UpdateAsync(dto, id);
            return Ok(new { message = "Cập nhật bài viết thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _service.GetByIdAsync(id);
            if (post == null) return NotFound();
            await _service.DeleteAsync(post);
            return Ok("Post deleted successfully.");
        }

        [HttpGet("Approve")]
        public async Task<IActionResult> GetApproval() =>
            Ok(await _service.GetApprovalAsync());

        [HttpPut("status/{postId}")]
        public async Task<IActionResult> UpdateStatus(int postId, [FromBody] UpdateBlogStatusDTO dto)
        {
            await _service.UpdatePostStatusAsync(postId, dto.Status);
            return Ok(new { Status = (int)dto.Status });
        }
    }
}
