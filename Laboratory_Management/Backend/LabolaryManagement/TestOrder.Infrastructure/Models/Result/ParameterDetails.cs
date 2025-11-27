using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Infrastructure.Models.Result
{
    public class ParameterDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        
        public string ReferenceRange { get; set; } = string.Empty;

        public bool? IsNormal { get; set; }
    }
}
