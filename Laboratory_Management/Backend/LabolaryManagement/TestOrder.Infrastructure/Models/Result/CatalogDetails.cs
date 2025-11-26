using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Infrastructure.Models.Result
{
    public class CatalogDetails
    {
        public string CatalogId { get; set; } = string.Empty;
        public string CatalogName { get; set; } = string.Empty;

        public string CatalogDescription { get; set; } = string.Empty;

        public List<ParameterDetails> Parameters { get; set; } = new List<ParameterDetails>();
    }
}
