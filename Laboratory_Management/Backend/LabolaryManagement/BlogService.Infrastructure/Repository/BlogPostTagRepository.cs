using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BlogService.Infrastructure.Models;
using BlogService.Infrastructure.Base;

namespace BlogService.Infrastructure.Repository
{
    public class BlogPostTagRepository : GenericRepository<BlogPostTag>
    { 
        public BlogPostTagRepository(DBContext context) : base(context)
        {
        }

        public async Task<List<BlogPostTag>> GetTagsByBlogPostIdAsync(int blogPostId)
        {
            return await _context.Set<BlogPostTag>().Where(bpt => bpt.PostId == blogPostId)
                                               .Include(bpt => bpt.Tag)
                                               .ToListAsync();
        }

        public async Task<List<BlogPostTag>> GetPostByTags (int tagId)
        {
            return await _context.Set<BlogPostTag>().Where(bpt => bpt.TagId == tagId)
                                               .Include(bpt => bpt.Post)
                                               .ToListAsync();
        }
    }
}
