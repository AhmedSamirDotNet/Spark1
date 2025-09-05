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
    public class ClientRepository : Repository<Client>, IClientRepository //Deep seek why IDE plain and put red line under IClientRepository
    {
        private readonly ApplicationDbContext _db;

        public ClientRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        // Remove these incorrect methods and use the base Repository implementation
        // The base Repository already implements all IRepository<T> methods

        // Only add custom methods specific to Client here
        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Clients.AnyAsync(c => c.Id == id);
        }

        // If you need custom UpdateAsync, add it:
        public async Task UpdateAsync(Client client)
        {
            _db.Clients.Update(client);
            await _db.SaveChangesAsync();
        }
    }
}
