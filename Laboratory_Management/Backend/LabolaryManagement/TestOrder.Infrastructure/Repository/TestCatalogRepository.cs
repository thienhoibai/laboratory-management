using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class TestCatalogRepository : GenericRepository<TestCatalog>
    {
        public TestCatalogRepository(Data.TestOrderDBContext context) : base(context)
        {
        }
        public async Task<(IEnumerable<TestCatalog> items, int totalItems)> GetAllPagedAsync(int page, int pageSize, string? search)
        {
            var query = _context.TestCatalogs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.TestName.Contains(search));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.TestName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<List<int>> GetParamtersByCatalogId (int id)
        {
            var catalog = await _context.TestCatalogs
                .Include(c => c.Parameters)
                .FirstOrDefaultAsync(c => c.CatalogId == id);
            if (catalog == null)
                throw new KeyNotFoundException($"Catalog with id {id} not found.");
            return catalog.Parameters.Select(p => p.ParameterId).ToList();

        }

        public async Task<TestCatalog> AddParameter(int catalogId, List<int> parameterId)
        {
            var catalog = await _context.TestCatalogs
                .Include(c => c.Parameters)
                .FirstOrDefaultAsync(c => c.CatalogId == catalogId);
            if (catalog == null)
                throw new KeyNotFoundException($"Catalog with id {catalogId} not found.");
            var parameters = await _context.TestParameters
                .Where(p => parameterId.Contains(p.ParameterId))
                .ToListAsync();
            foreach (var parameter in parameters)
            {
                if (!catalog.Parameters.Any(p => p.ParameterId == parameter.ParameterId))
                {
                    catalog.Parameters.Add(parameter);
                }
            }
            await _context.SaveChangesAsync();
            return catalog;

        }




        public async Task UpdateCatalogAsync(int id, string description, double price)
        {
            var catalog = await _context.TestCatalogs.FirstOrDefaultAsync(c => c.CatalogId == id);
            if (catalog == null)
                throw new KeyNotFoundException($"Catalog with id {id} not found.");

            catalog.Description = description;
            catalog.Price = price;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveParametersAsync(int catalogId, List<int> parameterIds)
        {
            var catalog = await _context.TestCatalogs
                .Include(c => c.Parameters)
                .FirstOrDefaultAsync(c => c.CatalogId == catalogId);
            if (catalog == null)
                throw new KeyNotFoundException($"Catalog with id {catalogId} not found.");
            var parametersToRemove = catalog.Parameters
                .Where(p => parameterIds.Contains(p.ParameterId))
                .ToList();
            foreach (var parameter in parametersToRemove)
            {
                catalog.Parameters.Remove(parameter);
            }
            await _context.SaveChangesAsync();
        }
    }
}
