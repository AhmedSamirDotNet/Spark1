using Spark.Models;

namespace Spark.DataAccess.Repository.IRepository
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByClientIdAsync(int clientId, CancellationToken cancellationToken = default);
        Task<Booking?> GetBookingWithDetailsAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<Booking> CreateBookingAsync(int clientId, int billboardId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    }
}