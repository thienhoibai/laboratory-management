using Microsoft.AspNetCore.Http;
namespace BlogService.Presentation.Controllers

{
   public class BlogPostCreateRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int CategoryId { get; set; }
        public Guid ? AuthorId { get; set; }
        public IFormFile? Image { get; set; }
    }

}
