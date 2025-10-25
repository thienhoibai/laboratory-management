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
        public TimeBlockRepository() : base()
        {
        }
    }
}
