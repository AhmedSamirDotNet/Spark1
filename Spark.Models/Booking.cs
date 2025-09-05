using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // Relation with Client
        public int? ClientId { get; set; }
        [ValidateNever]
        public Client? Client { get; set; }

        // Relation with Billboard
        public int? BillboardId { get; set; }
        [ValidateNever]
        public Billboard? Billboard { get; set; }

        // Extra booking info
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } //deep seek i want to delete this line becase i want to get all billboard from the client that is annisgn to them client book many billboards what do you think?


    }
}
