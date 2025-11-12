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
            if (dto.CatalogId == null || !dto.CatalogId.Any())
                throw new InvalidOperationException("Danh sách CatalogId không được trống.");

            var newEntities = dto.CatalogId
                .Select(id => new CatalogBundle
                {
                    BundleId = dto.BundleId,
                    CatalogId = id
                })
                .ToList();

            await _repository.AddRangeAsync(newEntities);
        }



        public async Task RemoveCatalogFromBundleAsync(int bundleId, List<int> catalogIds)
        {
            foreach (var catalogId in catalogIds)
            {
                await _repository.DeleteAsync(bundleId, catalogId);
            }
        }
    }
}
