using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Infrastructure.Models.Result
{
    public class ResultDetails
    {
        public string BookingCode { get; set; } = string.Empty;
        public List<CatalogDetails> Catalogs { get; set; } = new List<CatalogDetails>();
    }
}
