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
        private readonly TestParameterRepository _parameterRepository;

        public TestCatalogService(TestCatalogRepository repository,
                                  TestParameterRepository _parameterRepository)
        {
            _repository = repository;
            this._parameterRepository = _parameterRepository;
        }



        public async Task<object> GetAllCatalogAsync(int page, int pageSize, string? search)
        {
            // Chuẩn hoá phân trang: tránh OFFSET âm (PostgreSQL báo lỗi) và chia cho 0
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalItems) = await _repository.GetAllPagedAsync(page, pageSize, search);
            List<TestCatalogResponseDTO> catalogDTOs = new List<TestCatalogResponseDTO>();

            foreach (var catalog in items)
            {
                var catalogDTO = new TestCatalogResponseDTO
                {
                    Id = catalog.CatalogId,
                    CatalogName = catalog.TestName,
                    Description = catalog.Description,
                    Price = catalog.Price,
                    Parameters = new List<TestParameterDTO>()
                };
                List<int> parameterIds = await _repository.GetParamtersByCatalogId(catalog.CatalogId);
                foreach (var paramId in parameterIds)
                {
                    var parameter = await _parameterRepository.GetByIdAsync(paramId);
                    if (parameter != null)
                    {
                        catalogDTO.Parameters.Add(new TestParameterDTO
                        {
                            ParameterName = parameter.ParameterName,
                            Unit = parameter.Unit,
                            ReferenceRange = parameter.ReferenceRange
                        });
                    }

                }
                catalogDTOs.Add(catalogDTO);
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

            var catalogDTO = new TestCatalogResponseDTO
            {
                Id = catalog.CatalogId,
                CatalogName = catalog.TestName,
                Description = catalog.Description,
                Price = catalog.Price,
                Parameters = new List<TestParameterDTO>()
            };
            List<int> parameterIds = await _repository.GetParamtersByCatalogId(catalog.CatalogId);
            foreach (var paramId in parameterIds)
            {
                var parameter = await _parameterRepository.GetByIdAsync(paramId);
                if (parameter != null)
                {
                    catalogDTO.Parameters.Add(new TestParameterDTO
                    {
                        ParameterName = parameter.ParameterName,
                        Unit = parameter.Unit,
                        ReferenceRange = parameter.ReferenceRange
                    });
                }

            }
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

        public async Task DeleteCatalogAsync(int id)
        {
            var catalog =  await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(catalog);
        }

        internal double GetPriceForMultipleTests(List<int> testIds)
        {
            double prices = 0d;
            foreach (var id in testIds)
            {
                var catalog = _repository.GetByIdAsync(id).Result;
                prices += catalog.Price;
            }
                return prices;
        }
    }
}
