using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class CatalogParameterRepository : GenericRepository<CatalogParameter>
    {
        public CatalogParameterRepository(Data.TestOrderDBContext context) : base(context) { }

        public async Task<IEnumerable<CatalogParameter>> GetByParameterIdAsync(int parameterId)
        {
            return await _context.CatalogParameters
                .Include(cb => cb.Catalog)
                .Where(cb => cb.ParameterId == parameterId)
                .ToListAsync();
        }

        public async Task AddAsync(CatalogParameter entity)
        {
            _context.CatalogParameters.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}
