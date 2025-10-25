using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class CatalogParameterService
    {
        private readonly CatalogParameterRepository _repository;

        public CatalogParameterService(CatalogParameterRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CatalogParameter>> GetCatalogsByParameterAsync(int parameterId)
        {
            return await _repository.GetByParameterIdAsync(parameterId);
        }

        public async Task AddCatalogToParameterAsync(CatalogParameterDTO dto)
        {
            var entity = new CatalogParameter
            {
                ParameterId = dto.ParameterId,
                CatalogId = dto.CatalogId
            };

            await _repository.AddAsync(entity);
        }

       
    }
}
