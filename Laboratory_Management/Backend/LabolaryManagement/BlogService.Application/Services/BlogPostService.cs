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

        public Task<List<BlogPost>> GetAllAsync() => _repository.GetAllWithCategoryAsync();
        public Task<BlogPost?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task AddAsync(BlogPost post) => _repository.AddAsync(post);
        public Task UpdateAsync(BlogPost post) => _repository.UpdateAsync(post);
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

