using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlogService.Presentation.Controllers
{
    [Route("api")]
    [ApiController]
    [Tags("Blog Tags")]
    public class BlogTagController : ControllerBase
    {
        private readonly Application.Services.BlogTagService _blogTagService;
        public BlogTagController(Application.Services.BlogTagService blogTagService)
        {
            _blogTagService = blogTagService;
        }

        [HttpGet("blog-posts/{blogPostId}/tags")]
        [Authorize(Policy = "perm:BlogTag.BlogPost.View")]
        public async Task<IActionResult> GetTagsByBlogPostId(int blogPostId)
        {
            var result = await _blogTagService.GetTagsByBlogPostIdAsync(blogPostId);
            return Ok(result);
        }

        [HttpGet("tags/{tagId}/posts")]
        [Authorize(Policy = "perm:BlogTag.Tag.View")]
        public async Task<IActionResult> GetPostsByTagId(int tagId)
        {
            var result = await _blogTagService.GetPostsByTagIdAsync(tagId);
            return Ok(result);
        }

        [HttpPost("blog-posts/{blogPostId}/tags")]
        [Authorize(Policy = "perm:BlogTag.Create")]
        public async Task<IActionResult> AddTagsToBlogPost(int blogPostId, [FromBody] List<int> tagIds)
        {
            await _blogTagService.AddTagsToBlogPostAsync(blogPostId, tagIds);
            return Ok();
        }

        [HttpDelete("blog-posts/{blogPostId}/tags/{tagId}")]
        [Authorize(Policy = "perm:BlogTag.Delete")]
        public async Task<IActionResult> RemoveTagForBlogPost(int blogPostId, int tagId)
        {
            await _blogTagService.RemoveTagForBlogPostAsync(blogPostId, tagId);
            return Ok();
        }
    }
}
