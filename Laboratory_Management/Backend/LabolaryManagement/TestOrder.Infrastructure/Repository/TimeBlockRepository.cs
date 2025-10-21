using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;

namespace TestOrder.Infrastructure.Repository
{
    public class TimeBlockRepository : GenericRepository<Domain.TimeBlock>
    {
        public TimeBlockRepository(Data.TestOrderDBContext context) : base(context)
        {
        }
        public TimeBlockRepository() : base()
        {
        }
    }
}
