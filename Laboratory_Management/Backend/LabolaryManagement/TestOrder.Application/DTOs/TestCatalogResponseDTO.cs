using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs
{
    public class TestCatalogResponseDTO
    {
        public int Id { get; set; }
        public string CatalogName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }

        public List<TestParameterDTO> Parameters { get; set; } = new List<TestParameterDTO>();
    }
}
