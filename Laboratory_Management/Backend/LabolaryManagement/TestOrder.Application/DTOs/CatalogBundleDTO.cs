using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Application.DTOs
{
    public class CatalogBundleDTO

    {
        public int BundleId { get; set; }
        public List<int> CatalogId { get; set; }
    }
    
}
