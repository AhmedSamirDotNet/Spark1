using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System.Net;

namespace Spark.WebApi.Controllers.Admins
{

    namespace Spark.WebApi.Controllers.Admin
    {
        [Route("api/admin/[controller]")]
        [ApiController]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public class BillboardsController : ControllerBase
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger<BillboardsController> _logger;

            public BillboardsController(IUnitOfWork unitOfWork, ILogger<BillboardsController> logger)
            {
                _unitOfWork = unitOfWork;
                _logger = logger;
            }

            // GET: api/admin/billboards(GetAllBillboards)
            [HttpGet("GetAllBillboards")]
            public async Task<IActionResult> GetAllBillboards(
                [FromQuery] int page = 1,     //
                [FromQuery] int pageSize = 10,
                CancellationToken cancellationToken = default)
            {
                try 
                {
                    var billboards = await _unitOfWork.Billboard.GetAllAsync(
                        includeProperties: "Address",
                        cancellationToken: cancellationToken);

                    var totalCount = billboards.Count();
                    var pagedBillboards = billboards
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    return Ok(new
                    {
                        success = true,
                        data = pagedBillboards,
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
                    _logger.LogError(ex, "Error retrieving billboards");
                    return StatusCode((int)HttpStatusCode.InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while retrieving billboards."
                    });
                }
            }

            // GET: api/admin/billboards/{id}
            [HttpGet("{id}")]
            public async Task<IActionResult> GetBillboard(int id, CancellationToken cancellationToken)
            {
                try
                {
                    var billboard = await _unitOfWork.Billboard.GetAsync(
                        b => b.Id == id,
                        includeProperties: "Address",
                        cancellationToken: cancellationToken);

                    if (billboard == null)
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = $"Billboard with ID {id} not found."
                        });
                    }

                    return Ok(new
                    {
                        success = true,
                        data = billboard
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving billboard with ID {BillboardId}", id);
                    return StatusCode((int)HttpStatusCode.InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while retrieving the billboard."
                    });
                }
            }

            // Update the CreateBillboard method
            [HttpPost("CreateBillboard")]
            public async Task<IActionResult> CreateBillboard([FromBody] Billboard billboard, CancellationToken cancellationToken)
            {
                try
                {
                    if (billboard == null)
                        return BadRequest(new { success = false, message = "Billboard data is required." });

                    if (!ModelState.IsValid)
                        return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

                    // Check if address exists for one-to-one relationship
                    if (billboard.AddressId > 0)
                    {
                        var address = await _unitOfWork.Address.GetAsync(a => a.Id == billboard.AddressId, cancellationToken: cancellationToken);
                        if (address == null)
                            return BadRequest(new { success = false, message = "Address not found." });

                        // Check if address is already used by another billboard
                        var existingBillboardWithSameAddress = await _unitOfWork.Billboard.GetAsync(
                            b => b.AddressId == billboard.AddressId,
                            cancellationToken: cancellationToken);

                        if (existingBillboardWithSameAddress != null)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = "This address is already associated with another billboard."
                            });
                        }
                    }

                    await _unitOfWork.Billboard.AddAsync(billboard, cancellationToken);
                    await _unitOfWork.SaveAsync(cancellationToken);

                    // Reload with address
                    var createdBillboard = await _unitOfWork.Billboard.GetAsync(
                        b => b.Id == billboard.Id,
                        includeProperties: "Address",
                        cancellationToken: cancellationToken);

                    return CreatedAtAction(nameof(GetBillboard), new { id = billboard.Id }, new
                    {
                        success = true,
                        message = "Billboard created successfully.",
                        data = createdBillboard
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating billboard");
                    return StatusCode((int)HttpStatusCode.InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while creating the billboard."
                    });
                }
            }

            // Update the UpdateBillboard method
            [HttpPatch("UpdateBillboard/{id}")]
            public async Task<IActionResult> UpdateBillboard(int id, [FromBody] Billboard billboard, CancellationToken cancellationToken)
            {
                try
                {
                    if (id != billboard.Id)
                        return BadRequest(new { success = false, message = "ID mismatch." });

                    if (!ModelState.IsValid)
                        return BadRequest(new { success = false, message = "Invalid data.", errors = ModelState });

                    var existingBillboard = await _unitOfWork.Billboard.GetAsync(b => b.Id == id, cancellationToken: cancellationToken);
                    if (existingBillboard == null)
                        return NotFound(new { success = false, message = $"Billboard with ID {id} not found." });

                    // Check if address exists and is not used by another billboard
                    if (billboard.AddressId > 0 && billboard.AddressId != existingBillboard.AddressId)
                    {
                        var address = await _unitOfWork.Address.GetAsync(a => a.Id == billboard.AddressId, cancellationToken: cancellationToken);
                        if (address == null)
                            return BadRequest(new { success = false, message = "Address not found." });

                        var existingBillboardWithSameAddress = await _unitOfWork.Billboard.GetAsync(
                            b => b.AddressId == billboard.AddressId && b.Id != id,
                            cancellationToken: cancellationToken);

                        if (existingBillboardWithSameAddress != null)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = "This address is already associated with another billboard."
                            });
                        }
                    }

                    // Update properties including AddressId
                    existingBillboard.Code = billboard.Code;
                    existingBillboard.Description = billboard.Description;
                    existingBillboard.SubDescription = billboard.SubDescription;
                    existingBillboard.ImagePath = billboard.ImagePath;
                    existingBillboard.Size = billboard.Size;
                    existingBillboard.Highway = billboard.Highway;
                    existingBillboard.StartBooking = billboard.StartBooking;
                    existingBillboard.EndBooking = billboard.EndBooking;
                    existingBillboard.IsAvailable = billboard.IsAvailable;
                    existingBillboard.NumberOfFaces = billboard.NumberOfFaces;
                    existingBillboard.Type = billboard.Type;
                    existingBillboard.LocationURL = billboard.LocationURL;
                    existingBillboard.AddressId = billboard.AddressId;

                    await _unitOfWork.Billboard.UpdateAsync(existingBillboard);
                    await _unitOfWork.SaveAsync(cancellationToken);

                    // Reload with address
                    var updatedBillboard = await _unitOfWork.Billboard.GetAsync(
                        b => b.Id == id,
                        includeProperties: "Address",
                        cancellationToken: cancellationToken);

                    return Ok(new
                    {
                        success = true,
                        message = "Billboard updated successfully.",
                        data = updatedBillboard
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating billboard with ID {BillboardId}", id);
                    return StatusCode((int)HttpStatusCode.InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while updating the billboard."
                    });
                }
            }





            [HttpDelete("DeleteBillboard/{id}")]
            public async Task<IActionResult> DeleteBillboard(int id, CancellationToken cancellationToken)
            {
                try
                {
                    var billboard = await _unitOfWork.Billboard.GetAsync(
                        b => b.Id == id,
                        cancellationToken: cancellationToken);

                    if (billboard == null)
                        return NotFound(new { success = false, message = $"Billboard with ID {id} not found." });

                    // Check if there are any bookings associated with this billboard
                    var hasBookings = await _unitOfWork.Bookings.GetAllAsync(
                        b => b.BillboardId == id,
                        cancellationToken: cancellationToken);

                    if (hasBookings.Any())
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Cannot delete billboard because it has associated bookings. Please delete the bookings first."
                        });
                    }

                    // Remove the billboard (address will remain intact due to DeleteBehavior.Restrict)
                    await _unitOfWork.Billboard.RemoveAsync(billboard, cancellationToken);
                    await _unitOfWork.SaveAsync(cancellationToken);

                    return Ok(new
                    {
                        success = true,
                        message = "Billboard deleted successfully. Address remains unchanged."
                    });
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    // Foreign key constraint violation
                    _logger.LogError(ex, "Cannot delete billboard with ID {BillboardId} due to existing references", id);
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot delete billboard because it has associated bookings or other references."
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting billboard with ID {BillboardId}", id);
                    return StatusCode((int)HttpStatusCode.InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while deleting the billboard."
                    });
                }
            }
            #region DTO Update availability
            public class DtoAvailability
            {
                public bool IsAvailable { get; set; }
            }
            #endregion

            // PATCH: api/admin/billboards/UpdateAvailability/{id}/availability
            [HttpPatch("UpdateAvailability/{id}")]
            public async Task<IActionResult> UpdateAvailability(int id, [FromBody] DtoAvailability Availability, CancellationToken cancellationToken)
            {
                try
                {
                    var billboard = await _unitOfWork.Billboard.GetAsync(b => b.Id == id, cancellationToken: cancellationToken);
                    if (billboard == null)
                        return NotFound(new { success = false, message = $"Billboard with ID {id} not found." });

                    billboard.IsAvailable = Availability.IsAvailable;
                    await _unitOfWork.Billboard.UpdateAsync(billboard);
                    await _unitOfWork.SaveAsync(cancellationToken);

                    return Ok(new
                    {
                        success = true,
                        message = $"Billboard availability set to {(Availability.IsAvailable ? "Available" : "Unavailable")}.",
                        data = new { id, Availability }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating availability for billboard with ID {BillboardId}", id);
                    return StatusCode((int)HttpStatusCode.InternalServerError, new
                    {
                        success = false,
                        message = "An error occurred while updating billboard availability."
                    });
                }
            }

            [HttpGet("available")]
            public async Task<IActionResult> GetAvailableBillboards(CancellationToken cancellationToken)
            {
                try
                {
                    var availableBillboards = await _unitOfWork.Billboard.GetAllAsync(
                        filter: b => b.IsAvailable.GetValueOrDefault() &&
                        b.StartBooking <= DateTime.UtcNow &&
                        (b.EndBooking == null || b.EndBooking >= DateTime.UtcNow),
                        cancellationToken: cancellationToken);

                    return Ok(new
                    {
                        success = true,
                        data = availableBillboards
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving available billboards");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "An error occurred while retrieving available billboards."
                    });
                }
            }
        }
    }
}
