using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class TestParameterService
    {
        private readonly TestParameterRepository _repository;

        public TestParameterService(TestParameterRepository repository)
        {
            _repository = repository;
        }


        public async Task<object> GetAllParameterAsync(int page, int pageSize, string? search = null)
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

        public async Task<List<ParameterDisplayDTO>> GetParametersByIdsAsync(List<int> parameterIds)
        {
            var parameters = await _repository.GetListOfParametersByIdsAsync(parameterIds);
            if (parameters == null)
            {
                throw new Exception("No parameter found");
            } 

            var parameterDTOs = new List<ParameterDisplayDTO>();
            foreach (var parameter in parameters)
            {
                parameterDTOs.Add(new ParameterDisplayDTO
                {
                    Name = parameter.ParameterName,
                    Unit = parameter.Unit,
                    ReferenceRange = parameter.ReferenceRange,
                });

            }
            return parameterDTOs;

        }

        public async Task<TestParameter> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddParameterAsync(TestParameterDTO parameter)
        {
            var entity = new TestParameter
            {
                ParameterName = parameter.ParameterName,
                Unit = parameter.Unit,
                ReferenceRange = parameter.ReferenceRange,
                MinRange = parameter.MinRange,
                MaxRange = parameter.MaxRange

            };
            await _repository.AddParameterAsync(entity);
        }
    }
}
