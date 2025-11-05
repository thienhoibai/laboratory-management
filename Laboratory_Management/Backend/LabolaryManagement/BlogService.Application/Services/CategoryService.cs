using BlogService.Infrastructure.Models;
using BlogService.Infrastructure.Repository;
using BlogService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _repository;

        public CategoryService(CategoryRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Category>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Category?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task AddAsync(Category category) => _repository.AddAsync(category);
        public Task UpdateAsync(Category category) => _repository.UpdateAsync(category);
        public Task DeleteAsync(Category category) => _repository.DeleteAsync(category);
    }
}
