using BlogService.Application.DTOs;
using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Models;
using BlogService.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.Services
{
    public class BlogPostService
    {
        private readonly BlogPostRepository _repository;

        public BlogPostService(BlogPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BlogPostDTO>> GetAllAsync()
        {
            var posts = await _repository.GetAllWithCategoryAsync();

            var result = posts.Select(p => new BlogPostDTO
            {
                
                Title = p.Title,
                Content = p.Content,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                CreatedDate = p.CreatedDate,
                IsPublished = p.IsPublished,
                IsApproved = p.IsApproved,
                ThumbnailUrl = p.ThumbnailUrl
            }).ToList();

            return result;
        }
        public Task<BlogPost?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task AddAsync(BlogPost post) => _repository.AddAsync(post);
        public async Task UpdateAsync(UpdateBlogPostDTO dto, int id)
        {
            var post = await _repository.GetByIdAsync(id);
            if (post == null)
                throw new Exception("Bài viết không tồn tại.");

            post.Title = dto.Title ?? post.Title;
            post.Content = dto.Content ?? post.Content;
            post.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(post);
        }
        public Task DeleteAsync(BlogPost post) => _repository.DeleteAsync(post);
        public Task<List<BlogPost>> GetPendingApprovalAsync() => _repository.GetPendingApprovalAsync();

        public async Task AddAsync(BlogPostCreateDTO dto)
        {
            var post = new BlogPost
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.Now,
                IsPublished = dto.IsPublished ?? false,
                IsApproved = false,
                ThumbnailUrl = dto.ThumbnailUrl
            };

            await _repository.AddAsync(post);
        }
    }
}

