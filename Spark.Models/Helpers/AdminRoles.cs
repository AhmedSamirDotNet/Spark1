using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Models.Helpers
{
    public static class AdminRoles
    {
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";
        public const string Employee = "Employee";

        public static readonly string[] AllRoles = { Admin, SuperAdmin, Employee };
    }
}
