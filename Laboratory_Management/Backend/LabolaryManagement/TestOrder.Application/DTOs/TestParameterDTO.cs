using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Application.DTOs
{
    public class TestParameterDTO
    {
        public string? ParameterName { get; set; }
        public string? Unit { get; set; }
        public string? ReferenceRange { get; set; }

        public int CatalogId { get; set; }
        


    }
}
