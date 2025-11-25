using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Data;

namespace TestOrder.Infrastructure.Repository
{
    public class TestResultRepository : GenericRepository<TestResult>
    {
        public TestResultRepository(TestOrderDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TestResult>> GetResultByBookingTestNoAsync(long bookingTestNo)
        {
            return await Task.Run(() =>
            {
                return _context.Set<TestResult>()
                               .Where(tr => tr.TestBookingNo == bookingTestNo)
                               .AsEnumerable();
            });
        }
    }
}
