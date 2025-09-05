using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? LogoPath { get; set; }

        // Reports related to this client
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
    }
}
