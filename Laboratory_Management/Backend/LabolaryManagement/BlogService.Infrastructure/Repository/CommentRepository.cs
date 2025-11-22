using BlogService.Infrastructure.Base;
using BlogService.Infrastructure.Data;
using BlogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogService.Infrastructure.Repository
{
    public class CommentRepository : GenericRepository<Comment>
    {
        public CommentRepository(DBContext context) : base(context) { }

     
        public async Task<List<Comment>> GetByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

     
        public async Task<List<Comment>> GetByUserIdAsync(int userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Comment>> SearchAsync(string keyword)
        {
            return await _context.Comments
                .Where(c => !string.IsNullOrEmpty(c.Content) &&
                            c.Content.Contains(keyword))
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}
