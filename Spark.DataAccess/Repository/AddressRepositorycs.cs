using Microsoft.EntityFrameworkCore;
using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        private readonly ApplicationDbContext _db;

        public AddressRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Address>> GetAllAddressesWithDetailsAsync()
        {
            return await _db.Addresses
                 // لو في علاقة User
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _db.Addresses
                
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAsync(Address entity)
        {
            _db.Addresses.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}
