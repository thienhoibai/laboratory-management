using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class TestParameterRepository : GenericRepository<TestParameter>
    {
        public TestParameterRepository(Data.TestOrderDBContext context) : base(context)
        {
        }
        public async Task<(IEnumerable<TestParameter> Items, int TotalItems)> GetAllPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.TestParameters.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ParameterName.Contains(search));
            }

            var totalItems = await query.CountAsync();

            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return (items, totalItems);
        }

        public async Task<List<TestParameter>> GetListOfParametersByIdsAsync(List<int> parameterIds)
        {
            return await _context.TestParameters
                                 .Where(tp => parameterIds.Contains(tp.ParameterId))
                                 .ToListAsync();
        }


        public async Task AddParameterAsync(TestParameter parameter)
        {
            await _context.TestParameters.AddAsync(parameter);
            await _context.SaveChangesAsync();
        }

    }
}
