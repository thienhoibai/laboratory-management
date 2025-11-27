using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace BlogService.Presentation.Controllers

{
   public class BlogPostCreateRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "AuthorId is required")]
        public Guid ? AuthorId { get; set; }
        public IFormFile? Image { get; set; }
    }

}
