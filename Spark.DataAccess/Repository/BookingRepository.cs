using Microsoft.EntityFrameworkCore;
using Spark.DataAccess.Data;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;

namespace Spark.DataAccess.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByClientIdAsync(
            int clientId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .AsNoTracking()
                .Where(b => b.ClientId == clientId)
                .Include(b => b.Client)
                .Include(b => b.Billboard)
                .ToListAsync(cancellationToken);
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Bookings
                .AsNoTracking()
                .Include(b => b.Client)
                .Include(b => b.Billboard)
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }

        public async Task<Booking> CreateBookingAsync(
            int clientId,
            int billboardId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var booking = new Booking
            {
                ClientId = clientId,
                BillboardId = billboardId,
                StartDate = startDate,
                EndDate = endDate
            };

            await _db.Bookings.AddAsync(booking, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return booking;
        }

        public async Task<Booking> UpdateAsync(
            Booking booking,
            CancellationToken cancellationToken = default)
        {
            // Attach the entity if it's not being tracked
            var existingBooking = _db.Bookings.Local.FirstOrDefault(b => b.Id == booking.Id);
            if (existingBooking == null)
            {
                _db.Bookings.Attach(booking);
                _db.Entry(booking).State = EntityState.Modified;
            }
            else
            {
                // If already tracked, update the values
                _db.Entry(existingBooking).CurrentValues.SetValues(booking);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return booking;
        }
    }
}