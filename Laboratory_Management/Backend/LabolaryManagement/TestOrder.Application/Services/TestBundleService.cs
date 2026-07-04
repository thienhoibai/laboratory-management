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

        public async Task<object> GetAllBundleAsync(int page = 1, int pageSize = 10, string? search = null)
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


        public async Task DeleteBundleAsync(int id)
        {
            await _repository.DeleteBundleAsync(id);
        }

        internal double GetBundlePriceByIdAsync(int id)
        {
            return _repository.GetBundlePrice(id);
        }
    }
}
