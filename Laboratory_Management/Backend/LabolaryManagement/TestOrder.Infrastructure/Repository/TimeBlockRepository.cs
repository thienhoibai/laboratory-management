using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class TimeBlockRepository : GenericRepository<TimeBlock>
    {
        public TimeBlockRepository(Data.TestOrderDBContext context) : base(context)
        {
        }

        public async Task<TimeBlock?> GetByTime(TimeOnly Time)
        {
            return await _context.Set<TimeBlock>()
                .FirstOrDefaultAsync(tb => tb.TimeBlock1 == Time);
        }
    }

}
