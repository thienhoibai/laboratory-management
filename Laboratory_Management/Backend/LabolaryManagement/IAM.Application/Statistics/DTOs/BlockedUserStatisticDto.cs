using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM.Application.Statistics.DTOs
{
    public class BlockedUserStatisticDto
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBlockedUsers { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
