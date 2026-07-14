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
        public Task<List<BlogPost>> GetAllWithCategoryAsync(
            Guid? authorId, int? status, int page, int pageSize, string? search)
        {
            return _repository.GetAllWithCategoryAsync(authorId, status, page, pageSize,search);
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
                CreatedDate = DateTime.UtcNow,
                IsPublished = dto.IsPublished ?? false,
                IsApproved = false,
                ImagePath = dto.ImagePath,
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
            post.CategoryId = dto.CategoryId ?? post.CategoryId;

            // Update ảnh nếu có ảnh mới
            if (!string.IsNullOrEmpty(dto.ImagePath))
                post.ImagePath = dto.ImagePath;

            // Update author nếu có
            if (dto.AuthorId.HasValue)
                post.AuthorId = dto.AuthorId.Value;

            post.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(post);
        }


        // Xóa
        public Task DeleteAsync(BlogPost post) =>
            _repository.DeleteAsync(post);

        // Lấy danh sách bài đã duyệt
        public async Task<List<BlogPostDTO>> GetApprovalAsync()
        {
            var posts = await _repository.GetApprovalAsync();

            return posts
            .Select(p => new BlogPostDTO
            {
                Title = p.Title,
                Content = p.Content,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                IsPublished = p.IsPublished,
                IsApproved = p.IsApproved,
                ImagePath = p.ImagePath
            }).ToList();
        }
        public async Task UpdatePostStatusAsync(int postId, BlogPostStatus status)
        {
            var post = await _repository.GetByIdAsync(postId);
            if (post == null) throw new Exception("Bài viết không tồn tại.");

            post.Status = (int)status;
            post.IsApproved = status == BlogPostStatus.Approved;// lưu enum dưới dạng int
            post.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(post);
        }




    }
}
