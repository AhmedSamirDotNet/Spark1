using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository
{
    public class ContactUsRepository : Repository<ContactUs>, IContactUsRepository
    {
        private readonly ApplicationDbContext _db;

        public ContactUsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Async version of Update
        public async Task UpdateAsync(ContactUs obj, CancellationToken cancellationToken = default)
        {
            _db.ContactUs.Update(obj);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
