using Spark.Models;

using Spark.Models;

namespace Spark.DataAccess.Repository.IRepository
{
    //public interface IAdminUserRepository : IRepository<AdminUser>
    public interface IAdminUserRepository : IRepository<AdminUser>
    {
        void Update(AdminUser obj);
        AdminUser? GetByUsername(string username);
        void Add(AdminUser adminUser); // تأكد من وجود هذا

    }
}