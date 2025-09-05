using Spark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IClientRepository : IRepository<Client>
{
    Task<bool> ExistsAsync(int id);
    Task UpdateAsync(Client client);
}
}
