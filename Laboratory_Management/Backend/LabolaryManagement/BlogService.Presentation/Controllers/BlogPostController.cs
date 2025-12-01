using BlogService.Application.Services;
using BlogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
            post.ImagePath = post.ImagePath != null ? Path.Combine(_env.ContentRootPath, post.ImagePath) : null;
            return post == null ? NotFound() : Ok(post);
        }

        [HttpPost]
        [Authorize(Policy = "perm:BlogPost.Create")]
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
        [Authorize(Policy = "perm:BlogPost.Update")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBlogPostRequest request)
        {
            var post = await _service.GetByIdAsync(id);
            if (post == null)
                return NotFound();

            string? newImagePath = post.ImagePath; 

            if (request.Image != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "Images");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
                var savePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                newImagePath = Path.Combine("Images", fileName);
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldImage = Path.Combine(Directory.GetCurrentDirectory(), post.ImagePath);
                    if (System.IO.File.Exists(oldImage))
                        System.IO.File.Delete(oldImage);
                }
            }

            // --- MAPPING DTO ---
            var dto = new UpdateBlogPostDTO
            {
                Title = request.Title,
                Content = request.Content,
                CategoryId = request.CategoryId,
                ImagePath = newImagePath
            };

            await _service.UpdateAsync(dto, id);

            return Ok(new { message = "Cập nhật bài viết thành công", image = newImagePath });
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:BlogPost.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _service.GetByIdAsync(id);
            if (post == null) return NotFound();
            await _service.DeleteAsync(post);
            return Ok("Post deleted successfully.");
        }

        [HttpGet("Approve")]
        [Authorize(Policy = "perm:BlogPost.Approved.View")]
        public async Task<IActionResult> GetApproval() =>
            Ok(await _service.GetApprovalAsync());

        [HttpPut("status/{postId}")]
        [Authorize(Policy = "perm:BlogPost.Status.Update")]
        public async Task<IActionResult> UpdateStatus(int postId, [FromBody] UpdateBlogStatusDTO dto)
        {
            await _service.UpdatePostStatusAsync(postId, dto.Status);
            return Ok(new { Status = (int)dto.Status });
        }
    }
}
