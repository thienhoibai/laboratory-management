using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlogService.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Thao tác Nhãn cho bài viết")]
    public class BlogTagController : ControllerBase
    {
        private readonly Application.Services.BlogTagService _blogTagService;
        public BlogTagController(Application.Services.BlogTagService blogTagService)
        {
            _blogTagService = blogTagService;
        }

        [HttpGet]
        [Route("GetTagsByBlogPostId/{blogPostId}")]
        //[Authorize(Policy = "perm:BlogTag.BlogPost.View")]
        public async Task<IActionResult> GetTagsByBlogPostId(int blogPostId)
        {
            var result = await _blogTagService.GetTagsByBlogPostIdAsync(blogPostId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetPostsByTagId/{tagId}")]
        //[Authorize(Policy = "perm:BlogTag.Tag.View")]
        public async Task<IActionResult> GetPostsByTagId(int tagId)
        {
            var result = await _blogTagService.GetPostsByTagIdAsync(tagId);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddTagsToBlogPost/{blogPostId}")]
        //[Authorize(Policy = "perm:BlogTag.Create")]
        public async Task<IActionResult> AddTagsToBlogPost(int blogPostId, [FromBody] List<int> tagIds)
        {
            await _blogTagService.AddTagsToBlogPostAsync(blogPostId, tagIds);
            return Ok();
        }

        [HttpDelete]
        [Route("RemoveTagForBlogPost")]
        //[Authorize(Policy = "perm:BlogTag.Delete")]
        public async Task<IActionResult> RemoveTagForBlogPost([FromQuery]int blogPostId, [FromQuery] int tagId)
        {
            await _blogTagService.RemoveTagForBlogPostAsync(blogPostId, tagId);
            return Ok();
        }
    }
}
