using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class CatalogBundleService
    {
        private readonly CatalogBundleRepository _repository;

        public CatalogBundleService(CatalogBundleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<object>> GetCatalogsByBundleAsync(int bundleId)
        {
            return await _repository.GetByBundleIdAsync(bundleId);
        }

        public async Task AddCatalogToBundleAsync(CatalogBundleDTO dto)
        {
            var entity = new CatalogBundle
            {
                BundleId = dto.BundleId,
                CatalogId = dto.CatalogId
            };

            await _repository.AddAsync(entity);
        }

        public async Task RemoveCatalogFromBundleAsync(int bundleId, int catalogId)
        {
            await _repository.DeleteAsync(bundleId, catalogId);
        }
    }
}
