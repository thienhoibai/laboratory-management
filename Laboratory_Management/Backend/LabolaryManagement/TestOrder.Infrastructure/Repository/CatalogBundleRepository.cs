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
            bool exists = await _context.CatalogBundles
                .AnyAsync(cb => cb.BundleId == entity.BundleId && cb.CatalogId == entity.CatalogId);

            if (exists)
            {
                throw new InvalidOperationException("Catalog đã tồn tại trong Bundle này.");
            }

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
        public async Task<bool> ExistsAsync(int bundleId, int catalogId)
        {
            return await _context.CatalogBundles
                .AnyAsync(cb => cb.BundleId == bundleId && cb.CatalogId == catalogId);
        }
        public async Task AddRangeAsync(IEnumerable<CatalogBundle> entities)
        {
            foreach (var entity in entities)
            {
                bool exists = await _context.CatalogBundles
                    .AnyAsync(cb => cb.BundleId == entity.BundleId && cb.CatalogId == entity.CatalogId);

                if (exists)
                {
                    throw new InvalidOperationException(
                        $"CatalogId {entity.CatalogId} đã tồn tại trong BundleId {entity.BundleId}"
                    );
                }
            }

            _context.CatalogBundles.AddRange(entities);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAllByBundleIdAsync(int bundleId)
        {
            var entities = _context.CatalogBundles.Where(cb => cb.BundleId == bundleId);
            _context.CatalogBundles.RemoveRange(entities);
            await _context.SaveChangesAsync();

        }
    }
}
