using BlogService.Application.Services;
using BlogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlogService.Application.DTOs;
using BlogService.Application.Enums;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Hosting;


namespace BlogService.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Bài viết")]
    public class BlogPostController : ControllerBase
    {
        private readonly BlogPostService _service;
        private readonly IWebHostEnvironment _env;

        public BlogPostController(IWebHostEnvironment env, BlogPostService service)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogs(
    [FromQuery] Guid? authorId,
    [FromQuery] int? status,
    [FromQuery] string? search,
    [FromQuery] int page,
    [FromQuery] int pageSize)
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
        public async Task<IActionResult> Create([FromForm] BlogPostCreateRequest request)
        {
            string imagePath = null;

            if (request.Image != null)
            {
                // Lấy folder Images trong project
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "Images");
                Directory.CreateDirectory(folder); // tạo folder nếu chưa có

                var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
                var savePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                // Lưu path để trả về database (có thể dùng relative path)
                imagePath = Path.Combine("Images", fileName);
            }

            var dto = new BlogPostCreateDTO
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId,
                CategoryId = request.CategoryId,
                ImagePath = imagePath
            };

            await _service.AddAsync(dto);
            return Ok(new { message = "Bài viết đã tạo thành công" });
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
