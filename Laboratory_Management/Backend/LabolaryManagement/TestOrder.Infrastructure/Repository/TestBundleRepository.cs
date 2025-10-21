using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class TestBundleRepository : GenericRepository<TestBundle>
    {
        public TestBundleRepository(Data.TestOrderDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TestBundle>> GetActiveBundlesAsync()
        {
            return await _context.Set<TestBundle>()
                .Where(b => b.Price > 0)
                .OrderBy(b => b.BundleName)
                .ToListAsync();
        }

        public async Task UpdateBundleAsync(int id, string name, string description, double price)
        {
            var bundle = await _context.TestBundles.FirstOrDefaultAsync(b => b.BundleId == id);
            if (bundle == null)
                throw new KeyNotFoundException($"Bundle with id {id} not found.");

            bundle.BundleName = name;
            bundle.Description = description;
            bundle.Price = price;

            await _context.SaveChangesAsync();
        }
        public async Task DeleteBundleAsync(int id)
        {
            var bundle = await _context.TestBundles.FirstOrDefaultAsync(b => b.BundleId == id);
            if (bundle == null)
                throw new KeyNotFoundException($"Bundle with id {id} not found.");

            _context.TestBundles.Remove(bundle);
            await _context.SaveChangesAsync();
        }
    }
}
