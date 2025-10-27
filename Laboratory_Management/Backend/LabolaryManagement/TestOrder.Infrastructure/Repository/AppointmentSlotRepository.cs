using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Base;

namespace TestOrder.Infrastructure.Repository
{
    public class AppointmentSlotRepository : GenericRepository<AppointmentSlot>
    {
        public AppointmentSlotRepository(Data.TestOrderDBContext context) : base(context)
        {
        }

        public AppointmentSlotRepository() : base()
        {
        }
    }
}
