using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class TestCatalogService
    {
        private readonly TestCatalogRepository _repository;

        public TestCatalogService(TestCatalogRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TestCatalog>> GetAllCatalogAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<TestCatalog> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddCatalogAsync(TestCatalog catalog)
        {
            await _repository.AddAsync(catalog);
        }

        public async Task UpdateCatalogAsync(int id, string description, double price)
        {
            await _repository.UpdateCatalogAsync(id, description, price);
        }
    }
}
