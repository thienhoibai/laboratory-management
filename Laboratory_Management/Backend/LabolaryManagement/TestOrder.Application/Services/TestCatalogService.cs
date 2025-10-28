using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
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

        public async Task AddCatalogAsync(TestCatalogDTO catalog)
        {
            var entity = new TestCatalog
            {
                TestName = catalog.TestName,
                Description = catalog.Description,
                Price = catalog.Price,
            };
        }
        public async Task<TestCatalog> AddParameterAsync(int catalogId, List<int> parameterIds)
        {
            return await _repository.AddParameter(catalogId, parameterIds);
        }

        public async Task UpdateCatalogAsync(int id, string description, double price)
        {
            await _repository.UpdateCatalogAsync(id, description, price);
        }
    }
}
