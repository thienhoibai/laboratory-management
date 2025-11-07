using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class PaymentRepository : GenericRepository<PaymentEnvoice>
    {
        public PaymentRepository(TestOrderDBContext context) : base(context)
        {
        }
        public PaymentRepository() : base()
        {
        }
    }
}
