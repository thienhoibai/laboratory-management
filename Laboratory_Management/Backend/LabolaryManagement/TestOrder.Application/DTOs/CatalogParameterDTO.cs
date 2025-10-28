using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs
{
    public class CatalogParameterDTO
    {
        public int CatalogId { get; set; }

        public List<int> ParameterIds { get; set; } = new List<int>();


    }
}
