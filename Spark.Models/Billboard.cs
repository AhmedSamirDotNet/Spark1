using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models
{
    public class Billboard
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? SubDescription { get; set; }
        public string? ImagePath { get; set; }
        public string? Size { get; set; }
        public string? Highway { get; set; }
        public DateTime StartBooking { get; set; }
        public DateTime? EndBooking { get; set; }
        public bool? IsAvailable { get; set; }
        public int? NumberOfFaces { get; set; }
        public string? Type { get; set; }
        public string? LocationURL { get; set; }

        // One-to-One with Address (corrected)
        public int? AddressId { get; set; }

        [ForeignKey("AddressId")]
        [ValidateNever]
        public virtual Address? Address { get; set; }

        // Bookings instead of Reports
        public virtual ICollection<Booking>? Bookings { get; set; } = new List<Booking>();


    }
}
