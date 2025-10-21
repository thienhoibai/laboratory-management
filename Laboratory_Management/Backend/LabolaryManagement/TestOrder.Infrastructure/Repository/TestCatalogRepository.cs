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

        public async Task<IEnumerable<TestCatalog>> GetActiveCataLogAsync()
        {
            return await _context.Set<TestCatalog>()
                .Where(c => c.Price > 0)
                .OrderBy(c => c.TestName)
                .ToListAsync();
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
    }
}
