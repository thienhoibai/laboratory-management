using BlogService.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.Services
{
    public enum AddTagResult : int
    {
        Success = 0,
        TagAlreadyExists = 1
    }

    public class TagService
    {
        private readonly TagRepository _tagRepository;
        private readonly BlogTagService blogPostTagService;

        public TagService(TagRepository tagRepository, BlogTagService blogPostTagService)
        {
            _tagRepository = tagRepository;
            this.blogPostTagService = blogPostTagService;
        }
        public async Task<List<Infrastructure.Models.Tag>> GetAllTags()
        {
            return await _tagRepository.GetAllAsync();
        }
        public async Task<Infrastructure.Models.Tag?> GetTagById(int id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<int> AddTag(string tag)
        {
            var existingTag = await _tagRepository.GetAllAsync();
            if (existingTag != null)
            {

                foreach (var t in existingTag)
                {
                    if (t.TagName != null && t.TagName.Equals(tag, StringComparison.OrdinalIgnoreCase))
                    {
                        return (int)AddTagResult.TagAlreadyExists;
                    }
                }

            }

            var newTag = new Infrastructure.Models.Tag
            {
                TagName = tag
            };
            await _tagRepository.AddAsync(newTag);
            return (int)AddTagResult.Success;
        }

        public async Task<List<Infrastructure.Models.Tag>> GetTagsByIdPaged(int id, int pageNumber)
        {
            return await _tagRepository.GetByIdPagesAsync(id, pageNumber);
        }
        public async Task DeleteTag(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag != null)
            {
                await _tagRepository.DeleteAsync(tag);
            }

        }

        public async Task UpdateTag(int id, string name)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag != null)
            {
                tag.TagName = name;
                await _tagRepository.UpdateAsync(tag);
                blogPostTagService.RemoveTagForAllBlogPostsAsync(id).Wait();
            }
        }
    }
    }
