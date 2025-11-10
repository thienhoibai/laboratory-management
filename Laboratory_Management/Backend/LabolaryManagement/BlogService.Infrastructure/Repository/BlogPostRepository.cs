using BlogService.Infrastructure.Base;
using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Infrastructure.Repository
{
    public class BlogPostRepository : GenericRepository<BlogPost>
    {
        public BlogPostRepository(DBContext context) : base(context) { }

        public async Task<List<BlogPost>> GetAllWithCategoryAsync()
        {
            return await _context.BlogPosts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetPendingApprovalAsync()
        {
            return await _context.BlogPosts
                .Include(p => p.Category)
                .Where(p => p.IsApproved == false)
                .ToListAsync();
        }
    }
    }

