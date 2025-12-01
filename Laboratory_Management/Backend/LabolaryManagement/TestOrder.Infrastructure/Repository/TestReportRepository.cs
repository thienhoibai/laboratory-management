using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class TestReportRepository : GenericRepository<TestReport>
    {
        public TestReportRepository(Data.TestOrderDBContext context) : base(context)
        {
        }
        public TestReportRepository() : base()
        {
        }

        public async Task<TestReport?> GetReportByBookingId(Guid bookingId)
        {
            return await Task.Run(() => _context.Set<TestReport>()
                .FirstOrDefault(tr => tr.BookingId == bookingId));
        }
    }
}
