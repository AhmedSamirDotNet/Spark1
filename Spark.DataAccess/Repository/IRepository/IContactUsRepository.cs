using Spark.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IContactUsRepository : IRepository<ContactUs>
    {
        // Async update method
        Task UpdateAsync(ContactUs obj, CancellationToken cancellationToken = default);
    }
}
