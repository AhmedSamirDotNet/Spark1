using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models
{
    public class AdminUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string PasswordHash { get; set; } // هش كلمة السر، لا نخزنها نص عادي

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public UserRole Role { get; set; } = UserRole.Admin; 

    }
    public enum UserRole
    {
        SuperAdmin = 1,
        Admin=2,
        Employee=3
    }
}
