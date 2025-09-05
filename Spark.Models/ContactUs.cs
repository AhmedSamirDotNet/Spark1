using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models
{
    public class ContactUs
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Add Billboard association
        public int? BillboardId { get; set; } // Nullable if contact might not be billboard-specific

        [ForeignKey("BillboardId")]
        public virtual Billboard? Billboard { get; set; } // Navigation property

        // Optional: Add status to track if the inquiry has been handled
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Resolved
    }
}