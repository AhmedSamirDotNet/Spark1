using Microsoft.EntityFrameworkCore;
using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;

namespace Spark.DataAccess.Repository
{
    public class BillboardRepository : Repository<Billboard>, IBillboardRepository
    {
        private readonly ApplicationDbContext _db;

        public BillboardRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Billboard>> GetAvailableBillboardsAsync()
        {
            return await _db.Billboards
                .Where(b => b.IsAvailable.HasValue &&
           b.IsAvailable.Value &&
           b.StartBooking <= DateTime.UtcNow &&
           (b.EndBooking == null || b.EndBooking >= DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<Billboard?> GetByCodeAsync(string code)
        {
            return await _db.Billboards
                .FirstOrDefaultAsync(b => b.Code == code);
        }

        public async Task UpdateAsync(Billboard entity)
        {
            _db.Billboards.Update(entity);
            await _db.SaveChangesAsync();
        }
        public async Task<Billboard> GetBillboardWithAddressAsync(int id)
        {
            return await _db.Billboards
                .Include(b => b.Address)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Billboard>> GetAllBillboardsWithAddressAsync()
        {
            return await _db.Billboards
                .Include(b => b.Address)
                .ToListAsync();
        }

    }
}
