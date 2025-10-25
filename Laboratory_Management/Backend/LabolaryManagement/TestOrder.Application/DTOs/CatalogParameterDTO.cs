using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Application.DTOs
{
    public class CatalogParameterDTO
    {
        public int CatalogId { get; set; }
        public int ParameterId { get; set; }
    }
}
