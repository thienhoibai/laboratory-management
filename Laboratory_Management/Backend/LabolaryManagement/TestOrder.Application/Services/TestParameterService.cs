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
            // Chuẩn hoá phân trang: tránh OFFSET âm (PostgreSQL báo lỗi) và chia cho 0
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

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
                MinRange = parameter.MinRange,
                MaxRange = parameter.MaxRange

            };
            await _repository.AddParameterAsync(entity);
        }
        public async Task RemoveParameterAsync(int id)
        {
            var parameter = await _repository.GetByIdAsync(id);
            if (parameter == null)
                throw new Exception("Parameter not found");

            await _repository.RemoveParameterAsync(parameter);
        }

    }
}
