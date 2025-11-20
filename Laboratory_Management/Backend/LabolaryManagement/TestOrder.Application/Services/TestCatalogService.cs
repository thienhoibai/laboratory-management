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

        TestCatalogResponseDTO MapToDTO(TestCatalog catalog)
        {
            var dto = new TestCatalogResponseDTO
            {
                Id = catalog.CatalogId,
                CatalogName = catalog.TestName,
                Description = catalog.Description,
                Price = catalog.Price,
                Parameters = new List<TestParameterDTO>()
            };
            foreach (var param in catalog.Parameters)
            {
                dto.Parameters.Add(new TestParameterDTO
                {
                    ParameterName = param.ParameterName,
                    Unit = param.Unit,
                    ReferenceRange = param.ReferenceRange
                });
            }
            return dto;
        }

        public async Task<object> GetAllCatalogAsync(int page, int pageSize, string? search)
        {
            var (items, totalItems) = await _repository.GetAllPagedAsync(page, pageSize, search);

            

            List<TestCatalogResponseDTO> catalogDTOs = new List<TestCatalogResponseDTO>();

            foreach (var catalog in items)
            {
                catalogDTOs.Add(MapToDTO(catalog));
            }

            return new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                catalogDTOs
            };
        }

        public async Task<TestCatalogResponseDTO> GetByIdAsync(int id)
        {
            var catalog = await _repository.GetByIdAsync(id);
            var catalogDTO = MapToDTO(catalog);
            return catalogDTO;
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

        public async Task RemoveParameterAsync(int catalogId, List<int> parameterId)
        {
            if (parameterId == null || parameterId.Count == 0)
                return;
            await _repository.RemoveParametersAsync(catalogId, parameterId);


        }
    }
}
