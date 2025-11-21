using BlogService.Infrastructure.Base;
using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Infrastructure.Repository
{
    public class BlogPostRepository : GenericRepository<BlogPost>
    {
        public BlogPostRepository(DBContext context) : base(context) { }

        public async Task<List<BlogPost>> GetAllWithCategoryAsync(Guid? authorId, int? status, int page, int pageSize,string? search)

        {
            var query = _context.BlogPosts
                .Include(p => p.Category)
                .AsQueryable();

            if (authorId.HasValue)
                query = query.Where(p => p.AuthorId == authorId.Value);
            if  (!string.IsNullOrEmpty(search))
                    query = query.Where(p =>
                        p.Title!.Contains(search) ||
                        p.Category!.CategoryName!.Contains(search));
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p =>
                    p.Title!.Contains(search) ||
                    p.Category!.CategoryName!.Contains(search));



            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<List<BlogPost>> GetApprovalAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.IsApproved == true || p.Status ==1)
                .Include(p => p.Category)            
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(int postId, UpdateStatus status)
        {
            var post = await _context.BlogPosts.FindAsync(postId);
            if (post != null)
            {
                post.Status = (int)status; // lưu enum dưới dạng int

                post.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

    }
}

