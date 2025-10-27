using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class CatalogBundleRepository : GenericRepository<CatalogBundle>
    {
        public CatalogBundleRepository(Data.TestOrderDBContext context) : base(context) { }

        public async Task<IEnumerable<CatalogBundle>> GetByBundleIdAsync(int bundleId)
        {
            return await _context.CatalogBundles
                .Include(cb => cb.Catalog)
                .Where(cb => cb.BundleId == bundleId)
                .ToListAsync();
        }

        public async Task AddAsync(CatalogBundle entity)
        {
            _context.CatalogBundles.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int bundleId, int catalogId)
        {
            var entity = await _context.CatalogBundles
                .FirstOrDefaultAsync(cb => cb.BundleId == bundleId && cb.CatalogId == catalogId);

            if (entity != null)
            {
                _context.CatalogBundles.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
