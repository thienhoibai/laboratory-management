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

    }
}
