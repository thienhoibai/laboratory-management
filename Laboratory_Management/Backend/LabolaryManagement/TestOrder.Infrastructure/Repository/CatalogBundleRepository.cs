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

        public async Task<IEnumerable<object>> GetByBundleIdAsync(int bundleId)
        {
            var rows = await _context.CatalogBundles
                .Include(cb => cb.Bundle)
                .Where(cb => cb.BundleId == bundleId)
                .Include(cb => cb.Catalog)
                .ToListAsync();

            var grouped = rows
                .GroupBy(cb => new {
                    cb.Bundle.BundleId,
                    cb.Bundle.BundleName,
                    cb.Bundle.Description,
                    cb.Bundle.Price
                })
                .Select(g => new {
                    g.Key.BundleId,
                    g.Key.BundleName,
                    g.Key.Description,
                    g.Key.Price,
                    Catalogs = g.Select(x => new {
                        x.Catalog.CatalogId,
                        x.Catalog.TestName,
                        x.Catalog.Description,
                        x.Catalog.Price
                    }).ToList()
                });

            return grouped.ToList();
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var rows = await _context.CatalogBundles
                .Include(cb => cb.Bundle)
                .Include(cb => cb.Catalog)
                .ToListAsync();

     
            var grouped = rows
                .GroupBy(cb => new
                {
                    cb.Bundle.BundleId,
                    cb.Bundle.BundleName,
                    cb.Bundle.Description,
                    cb.Bundle.Price
                })
                .Select(g => new
                {
                    g.Key.BundleId,
                    g.Key.BundleName,
                    g.Key.Description,
                    g.Key.Price,
                    Catalogs = g.Select(x => new
                    {
                        x.Catalog.CatalogId,
                        x.Catalog.TestName,
                        x.Catalog.Description,
                        x.Catalog.Price
                    }).ToList()
                });

            return grouped.ToList();
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
