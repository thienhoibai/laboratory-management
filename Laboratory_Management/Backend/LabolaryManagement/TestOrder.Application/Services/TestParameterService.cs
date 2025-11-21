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

            };
            await _repository.AddParameterAsync(entity);
        }
    }
}
