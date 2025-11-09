using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Infrastructure.Repository;

namespace BlogService.Application.Services
{
    public class BlogTagService
    {
        private readonly BlogPostTagRepository _blogPostTagRepository;

        public BlogTagService(BlogPostTagRepository blogPostTagRepository)
        {
            _blogPostTagRepository = blogPostTagRepository;
        }

        public async Task<List<Infrastructure.Models.BlogPostTag>> GetTagsByBlogPostIdAsync(int blogPostId)
        {
            return await _blogPostTagRepository.GetTagsByBlogPostIdAsync(blogPostId);
        }

        public async Task<List<Infrastructure.Models.BlogPostTag>> GetPostsByTagIdAsync(int tagId)
        {
            return await _blogPostTagRepository.GetPostByTags(tagId);
        }

        public async Task AddTagsToBlogPostAsync(int blogPostId, List<int> tagIds)
        {
            foreach (var tagId in tagIds)
            {
                var blogPostTag = new Infrastructure.Models.BlogPostTag
                {
                    PostId = blogPostId,
                    TagId = tagId
                };
                await _blogPostTagRepository.AddAsync(blogPostTag);
            }
        }

        public async Task RemoveTagForBlogPostAsync(int blogPostId, int tagId)
        {
            var blogPostTags = await _blogPostTagRepository.GetAllAsync();
            var blogPostTagToRemove = blogPostTags
                .FirstOrDefault(bpt => bpt.PostId == blogPostId && bpt.TagId == tagId);
            if (blogPostTagToRemove != null)
            {
                await _blogPostTagRepository.DeleteAsync(blogPostTagToRemove);
            }
        }

        public async Task RemoveTagForAllBlogPostsAsync(int tagId)
        {
            var blogPostTags = await _blogPostTagRepository.GetAllAsync();
            var blogPostTagsToRemove = blogPostTags
                .Where(bpt => bpt.TagId == tagId)
                .ToList();
            foreach (var blogPostTag in blogPostTagsToRemove)
            {
                await _blogPostTagRepository.DeleteAsync(blogPostTag);
            }
        }
    }
}
