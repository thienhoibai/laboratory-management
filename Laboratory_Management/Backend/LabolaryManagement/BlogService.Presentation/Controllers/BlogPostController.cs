using BlogService.Application.Services;
using BlogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlogService.Application.DTOs;

namespace BlogService.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostController : ControllerBase
    {
        private readonly BlogPostService _service;

        public BlogPostController(BlogPostService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

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
        public async Task<IActionResult> Update(int id,[FromBody] UpdateBlogPostDTO dto)
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApproval() =>
            Ok(await _service.GetPendingApprovalAsync());
    }
}
