using BlogService.Application.DTOs;
using BlogService.Application.Enums;
using BlogService.Infrastructure.Models;
using BlogService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Lấy tất cả bài viết (kèm Category)
        public async Task<List<BlogPostDTO>> GetAllAsync()
        {
            var posts = await _repository.GetAllWithCategoryAsync();

            return posts.Select(p => new BlogPostDTO
            {
                Title = p.Title,
                Content = p.Content,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                IsPublished = p.IsPublished,
                IsApproved = p.IsApproved,
                ThumbnailUrl = p.ThumbnailUrl
            }).ToList();
        }

        public Task<BlogPost?> GetByIdAsync(int id) =>
            _repository.GetByIdAsync(id);

        // Tạo mới bài viết (nhận DTO)
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

        // Cập nhật
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

        // Xóa
        public Task DeleteAsync(BlogPost post) =>
            _repository.DeleteAsync(post);

        // Lấy danh sách bài chờ duyệt
        public async Task<List<BlogPostDTO>> GetPendingApprovalAsync()
        {
            var posts = await _repository.GetPendingApprovalAsync();

            return posts.Select(p => new BlogPostDTO
            {
                Title = p.Title,
                Content = p.Content,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                IsPublished = p.IsPublished,
                IsApproved = p.IsApproved,
                ThumbnailUrl = p.ThumbnailUrl
            }).ToList();
        }
        public async Task UpdatePostStatusAsync(int postId, BlogPostStatus status)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null) throw new Exception("Bài viết không tồn tại.");

            post.Status = (int)status;
            post.IsApproved = status == BlogPostStatus.Approved;// lưu enum dưới dạng int
            post.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(post);
        }




    }
}
