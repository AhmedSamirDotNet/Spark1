using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }

        // Navigation to AdminUser
        [Required]
        [ForeignKey("AdminUser")]
        public int AdminUserId { get; set; }
        public AdminUser AdminUser { get; set; }

        [NotMapped]
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        // Mark IsActive as NotMapped since it's computed
        //[NotMapped]
        //public bool IsActive => !IsRevoked && !IsExpired;

    }
}
