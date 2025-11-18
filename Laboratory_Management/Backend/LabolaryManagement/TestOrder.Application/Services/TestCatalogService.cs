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

        public async Task<object> GetAllCatalogAsync(int page = 1, int pageSize = 10, string? search = null)
        {
            var (items, totalItems) = await _repository.GetAllPagedAsync(page, pageSize, search);

            return new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                items
            };
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
                Price = catalog.Price

            };
           await _repository.AddAsync(entity);

        }
        public async Task<TestCatalogResponseDTO> AddParameterAsync(int catalogId, List<int> parameterIds)
        {
            await _repository.AddParameter(catalogId, parameterIds);

            return await _repository.GetByIdAsync(catalogId)
                .ContinueWith( task =>
                {
                    var catalog = task.Result;
                    var responseDto = new TestCatalogResponseDTO
                    {
                        Id = catalog.CatalogId,
                        CatalogName = catalog.TestName,
                        Description = catalog.Description,
                        Price = catalog.Price,
                        Parameters = new List<TestParameterDTO>()
                    };
                    foreach (var param in catalog.Parameters)
                    {
                        responseDto.Parameters.Add(new TestParameterDTO
                        {
                            
                            ParameterName = param.ParameterName,
                            Unit = param.Unit,
                            ReferenceRange = param.ReferenceRange
                        });
                    }
                    return responseDto;
                });
        }

        public async Task UpdateCatalogAsync(int id, string description, double price)
        {
            await _repository.UpdateCatalogAsync(id, description, price);
        }
    }
}
