using Spark.Models;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
    }
}
