using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class VoucherRepository : GenericRepository<Voucher>
    {
        public VoucherRepository(TestOrderDBContext context) : base(context)
        {
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return await _context.Set<Voucher>()
                .FirstOrDefaultAsync(v => v.Code.ToUpper() == code.ToUpper());
        }
    }
}
