using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System.Net;

namespace Spark.WebApi.Controllers.Admins
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class BookingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IUnitOfWork unitOfWork, ILogger<BookingsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/admin/bookings
        [HttpGet]
        public async Task<IActionResult> GetAllBookings(
            [FromQuery] int? clientId = null,
            [FromQuery] int? billboardId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetAllAsync(
                    includeProperties: "Client,Billboard,Billboard.Address",
                    cancellationToken: cancellationToken);

                // Apply filters
                if (clientId.HasValue)
                    bookings = bookings.Where(b => b.ClientId == clientId.Value);

                if (billboardId.HasValue)
                    bookings = bookings.Where(b => b.BillboardId == billboardId.Value);

                if (startDate.HasValue)
                    bookings = bookings.Where(b => b.StartDate >= startDate.Value);

                if (endDate.HasValue)
                    bookings = bookings.Where(b => b.EndDate <= endDate.Value);

                var totalCount = bookings.Count();
                var pagedBookings = bookings
                    .OrderByDescending(b => b.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = pagedBookings,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving bookings."
                });
            }
        }

        // GET: api/admin/bookings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetAsync(
                    b => b.Id == id,
                    includeProperties: "Client,Billboard,Billboard.Address",
                    cancellationToken: cancellationToken);

                if (booking == null)
                    return NotFound(new { success = false, message = $"Booking with ID {id} not found." });

                return Ok(new { success = true, data = booking });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking with ID {BookingId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving the booking."
                });
            }
        }

        // POST: api/admin/bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking, CancellationToken cancellationToken)
        {
            try
            {
                if (booking == null)
                    return BadRequest(new { success = false, message = "Booking data is required." });

                // Validate client exists
                var client = await _unitOfWork.Client.GetAsync(c => c.Id == booking.ClientId, cancellationToken: cancellationToken);
                if (client == null)
                    return BadRequest(new { success = false, message = "Client not found." });

                // Validate billboard exists
                var billboard = await _unitOfWork.Billboard.GetAsync(b => b.Id == booking.BillboardId, cancellationToken: cancellationToken);
                if (billboard == null)
                    return BadRequest(new { success = false, message = "Billboard not found." });

                await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                var createdBooking = await _unitOfWork.Bookings.GetAsync(
                    b => b.Id == booking.Id,
                    includeProperties: "Client,Billboard,Billboard.Address",
                    cancellationToken: cancellationToken);

                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, new
                {
                    success = true,
                    message = "Booking created successfully.",
                    data = createdBooking
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while creating the booking."
                });
            }
        }

        // PUT: api/admin/bookings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] Booking booking, CancellationToken cancellationToken)
        {
            try
            {
                if (id != booking.Id)
                    return BadRequest(new { success = false, message = "ID mismatch." });

                var existingBooking = await _unitOfWork.Bookings.GetAsync(b => b.Id == id, cancellationToken: cancellationToken);
                if (existingBooking == null)
                    return NotFound(new { success = false, message = $"Booking with ID {id} not found." });

                // Validate client exists if changed
                if (existingBooking.ClientId != booking.ClientId)
                {
                    var client = await _unitOfWork.Client.GetAsync(c => c.Id == booking.ClientId, cancellationToken: cancellationToken);
                    if (client == null)
                        return BadRequest(new { success = false, message = "Client not found." });
                }

                // Validate billboard exists if changed
                if (existingBooking.BillboardId != booking.BillboardId)
                {
                    var billboard = await _unitOfWork.Billboard.GetAsync(b => b.Id == booking.BillboardId, cancellationToken: cancellationToken);
                    if (billboard == null)
                        return BadRequest(new { success = false, message = "Billboard not found." });
                }

                // Update properties
                existingBooking.ClientId = booking.ClientId;
                existingBooking.BillboardId = booking.BillboardId;
                existingBooking.StartDate = booking.StartDate;
                existingBooking.EndDate = booking.EndDate;

                await _unitOfWork.Bookings.UpdateAsync(existingBooking);
                await _unitOfWork.SaveAsync(cancellationToken);

                var updatedBooking = await _unitOfWork.Bookings.GetAsync(
                    b => b.Id == id,
                    includeProperties: "Client,Billboard,Billboard.Address",
                    cancellationToken: cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Booking updated successfully.",
                    data = updatedBooking
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking with ID {BookingId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while updating the booking."
                });
            }
        }

        // DELETE: api/admin/bookings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id, CancellationToken cancellationToken)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetAsync(b => b.Id == id, cancellationToken: cancellationToken);
                if (booking == null)
                    return NotFound(new { success = false, message = $"Booking with ID {id} not found." });

                await _unitOfWork.Bookings.RemoveAsync(booking);
                await _unitOfWork.SaveAsync(cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Booking deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking with ID {BookingId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while deleting the booking."
                });
            }
        }

        // GET: api/admin/bookings/client/{clientId}
        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetBookingsByClient(int clientId, CancellationToken cancellationToken)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetAllAsync(
                    b => b.ClientId == clientId,
                    includeProperties: "Billboard,Billboard.Address",
                    cancellationToken: cancellationToken);

                return Ok(new { success = true, data = bookings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for client ID {ClientId}", clientId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving client bookings."
                });
            }
        }

        // GET: api/admin/bookings/billboard/{billboardId}
        [HttpGet("billboard/{billboardId}")]
        public async Task<IActionResult> GetBookingsByBillboard(int billboardId, CancellationToken cancellationToken)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetAllAsync(
                    b => b.BillboardId == billboardId,
                    includeProperties: "Client",
                    cancellationToken: cancellationToken);

                return Ok(new { success = true, data = bookings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for billboard ID {BillboardId}", billboardId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving billboard bookings."
                });
            }
        }
    }
}