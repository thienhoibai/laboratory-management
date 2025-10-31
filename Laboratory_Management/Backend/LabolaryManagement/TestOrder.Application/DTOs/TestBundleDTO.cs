using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Application.DTOs
{
    public class TestBundleDTO
    {
        public string? BundleName { get; set; }
        public string? Description { get; set; }
        public double? Price { get; set; }
        public bool? IsActive { get; set; }
        
    }
}
