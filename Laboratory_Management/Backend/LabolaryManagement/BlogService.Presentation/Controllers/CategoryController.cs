using BlogService.Application.Services;
using BlogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BlogService.Application.DTOs; 

namespace BlogService.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Danh mục bài viết")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;

        public CategoryController(CategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        //[Authorize(Policy = "perm:BlogCategory.List")]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        //[Authorize(Policy = "perm:BlogCategory.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        [HttpPost]
        //[Authorize(Policy = "perm:BlogCategory.Create")]
        public async Task<IActionResult> Create(CategoryDTO dto)
        {
            var category = new Category
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description,
                CreatedDate = DateTime.Now
            };

            await _service.AddAsync(category);
            return Ok("Category created successfully.");
        }

        [HttpPut("{id}")]
        //[Authorize(Policy = "perm:BlogCategory.Update")]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.CategoryId) return BadRequest();
            await _service.UpdateAsync(category);
            return Ok("Category updated successfully.");
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = "perm:BlogCategory.Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            await _service.DeleteAsync(category);
            return Ok("Category deleted successfully.");
        }
    }
}
