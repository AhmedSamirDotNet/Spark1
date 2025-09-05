using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;

namespace Spark.DataAccess.Repository
{
    public class AdminUserRepository : Repository<AdminUser>, IAdminUserRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(AdminUser obj)
        {
            _db.AdminUsers.Update(obj);
        }

        public AdminUser? GetByUsername(string username)
        {
            return _db.AdminUsers.FirstOrDefault(u => u.UserName == username);
        }

        public void Add(AdminUser adminUser)
        {
            _db.AdminUsers.Add(adminUser);
        }
    }
}
