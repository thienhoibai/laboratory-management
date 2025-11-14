using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs
{
    public enum ResponseCode : int
    {
        Success = 0,
        NotFound = -1,
        BadInstanceState = -2,
    }
    public class ResponseMessage
    {
        public object? InstancesCode { get; set; }
        public ResponseCode ResponseCode { get; set; }

        public string? Message { get; set; }
    }
}
