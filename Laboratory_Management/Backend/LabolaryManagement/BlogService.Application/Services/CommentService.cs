using BlogService.Application.DTOs;
using BlogService.Infrastructure.Models;
using BlogService.Infrastructure.Repository;

namespace BlogService.Application.Services
{
    public class CommentService
    {
        private readonly CommentRepository _repository;

        public CommentService(CommentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CommentDTO>> GetByPostIdAsync(int postId)
        {
            var comments = await _repository.GetByPostIdAsync(postId);

            return comments.Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
                IsUpdated = c.IsUpdated
            }).ToList();
        }


        public Task<Comment?> GetByIdAsync(int id) =>
            _repository.GetByIdAsync(id);


        public async Task AddAsync(CommentDTO dto)
        {
            var comment = new Comment
            {
                PostId = dto.PostId,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatedDate = DateTime.UtcNow,
                IsUpdated = false
            };

            await _repository.AddAsync(comment);
        }


        public async Task UpdateAsync(int id, CommentDTO dto)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null)
                throw new Exception("Comment không tồn tại.");

            comment.Content = dto.Content ?? comment.Content;
            comment.IsUpdated = true;
            comment.CreatedDate = comment.CreatedDate; 

            await _repository.UpdateAsync(comment);
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null)
                throw new Exception("Comment không tồn tại.");

            await _repository.DeleteAsync(comment);
        }


        public async Task<List<CommentDTO>> SearchAsync(string keyword)
        {
            var comments = await _repository.SearchAsync(keyword);

            return comments.Select(c => new CommentDTO
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
                IsUpdated = c.IsUpdated
            }).ToList();
        }
    }
}
