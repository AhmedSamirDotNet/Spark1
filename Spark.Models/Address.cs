using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;


namespace Spark.Models
{
    public class Address
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        // Navigation property for one-to-one relationship
        [ValidateNever]
        public virtual Billboard? Billboard { get; set; }
    }
}