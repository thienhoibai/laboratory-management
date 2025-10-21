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

        public async Task<IEnumerable<TestParameter>> GetAllParametersAsync()
        {
            return await _context.TestParameters
                .OrderBy(p => p.ParameterName)
                .ToListAsync();
        }

        public async Task AddParameterAsync(TestParameter parameter)
        {
            await _context.TestParameters.AddAsync(parameter);
            await _context.SaveChangesAsync();
        }
    }
}
