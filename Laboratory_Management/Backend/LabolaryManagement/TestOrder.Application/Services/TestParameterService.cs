using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<TestParameter>> GetAllParametersAsync()
        {
            return await _repository.GetAllParametersAsync();
        }

        public async Task<TestParameter> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddParameterAsync(TestParameter parameter)
        {
            await _repository.AddParameterAsync(parameter);
        }
    }
}
