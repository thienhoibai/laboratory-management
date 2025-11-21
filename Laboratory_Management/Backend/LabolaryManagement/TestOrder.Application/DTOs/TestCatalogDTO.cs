using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs
{
    public class TestCatalogDTO
    {
        public string TestName { get; set; } = null!;

        public string? Description { get; set; }

        public double Price { get; set; }

        public List<int>? ParameterIds { get; set; }
    }
}
