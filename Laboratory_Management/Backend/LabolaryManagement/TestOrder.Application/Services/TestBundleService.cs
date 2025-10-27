using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class TestBundleService
    {
        private readonly TestBundleRepository _repository;

        public TestBundleService(TestBundleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TestBundle>> GetAllBundlesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<TestBundle> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddBundleAsync(TestBundleDTO dto)
        {
            var entity = new TestBundle
            {
                BundleName = dto.BundleName,
                Description = dto.Description,
                Price = dto.Price,
                IsActive = true
            };

            await _repository.AddAsync(entity);
        }

        public async Task UpdateBundleAsync(int id, string name, string description, double price)
        {
            await _repository.UpdateBundleAsync(id, name, description, price);
        }

        public async Task<IEnumerable<TestBundle>> GetActiveBundlesAsync()
        {
            return await _repository.GetActiveBundlesAsync();
        }
        public async Task DeleteBundleAsync(int id)
        {
            await _repository.DeleteBundleAsync(id);
        }
    }
}
