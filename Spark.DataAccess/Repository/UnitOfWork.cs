using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using System.Threading;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IContactUsRepository ContactUs { get; private set; }
        public IAdminUserRepository AdminUsers { get; private set; }  // اضافه هنا

        public IRefreshTokenRepository RefreshTokens { get; private set; }


        public IBookingRepository Bookings { get; private set; }
        public IClientRepository Client { get; private set; }
        public IBillboardRepository Billboard { get; private set; }
        public IAddressRepository Address { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            ContactUs = new ContactUsRepository(_db);
            AdminUsers = new AdminUserRepository(_db); // لازم تعمل Repository للـAdminUser
            RefreshTokens = new RefreshTokenRepository(_db);
            Address = new AddressRepository(_db);
            Bookings = new BookingRepository(_db);
            Client=new ClientRepository(_db);
            Billboard=new BillboardRepository(_db);
        }

        // Sync Save method
        public void Save()
        {
            _db.SaveChanges();
        }

        // Async Save method
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
