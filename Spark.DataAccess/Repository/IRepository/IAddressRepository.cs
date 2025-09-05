using Spark.Models;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IAddressRepository : IRepository<Address>
    {

        Task<IEnumerable<Address>> GetAllAddressesWithDetailsAsync();

        Task<Address?> GetByIdAsync(int id);

        Task UpdateAsync(Address entity);

    }
}
