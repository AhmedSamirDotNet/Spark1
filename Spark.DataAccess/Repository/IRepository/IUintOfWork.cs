using System.Threading;
using System.Threading.Tasks;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IContactUsRepository ContactUs { get; }
        IAdminUserRepository AdminUsers { get; }  // اضافه هنا

        IRefreshTokenRepository RefreshTokens { get; }

        //
        IAddressRepository Address { get; }
        IBillboardRepository Billboard { get; }
        IClientRepository Client { get; }
        IBookingRepository Bookings { get; }

        void Save(); // أضف هذا


        // Async version of Save
        Task SaveAsync(CancellationToken cancellationToken = default); // Async version
    }
}
