using Microsoft.AspNetCore.Mvc;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Net;
using static Spark.WebApi.Controllers.Shared.HomeController;

namespace Spark.WebApi.Controllers
{
    [Route("shared/api/[controller]")]
    [ApiController]
    public class BillboardsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BillboardsController> _logger;

        public BillboardsController(IUnitOfWork unitOfWork, ILogger<BillboardsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/billboards
        [HttpGet]
        public async Task<IActionResult> GetBillboards(
            [FromQuery] bool? availableOnly = null,
            [FromQuery] string? location = null,
            [FromQuery] string? highway = null,
            [FromQuery] string? type = null,
            [FromQuery] string? size = null,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null,
            [FromQuery] string? searchQuery = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Build dynamic filter based on query parameters
                Expression<Func<Billboard, bool>> filter = BuildFilterExpression(
                    availableOnly, location, highway, type, size, month, year, searchQuery);

                var billboards = await _unitOfWork.Billboard.GetAllAsync(
                    filter: filter,
                    includeProperties: "Address",
                    cancellationToken: cancellationToken);

                var totalCount = billboards.Count();
                var pagedBillboards = billboards
                    .OrderBy(b => b.Code)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var billboardDtos = pagedBillboards.Select(ConvertToDto).ToList();

                var filterOptions = await GetAvailableFilterOptions(cancellationToken);

                return Ok(new
                {
                    success = true,
                    data = billboardDtos,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    filters = new
                    {
                        availableOnly,
                        location,
                        highway,
                        type,
                        size,
                        month,
                        year,
                        searchQuery
                    },
                    availableFilters = filterOptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billboards");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving billboards."
                });
            }
        }

        // GET: api/billboards/available
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableBillboards(CancellationToken cancellationToken)
        {
            try
            {
                var availableBillboards = await _unitOfWork.Billboard.GetAllAsync(
                    filter: b => b.IsAvailable == true &&
                                 b.StartBooking <= DateTime.Now &&
                                 (b.EndBooking == null || b.EndBooking >= DateTime.Now),
                    includeProperties: "Address",
                    cancellationToken: cancellationToken);

                var billboardDtos = availableBillboards.Select(ConvertToDto).ToList();

                return Ok(new
                {
                    success = true,
                    data = billboardDtos
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

        // GET: api/billboards/{id}
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
                    return NotFound(new
                    {
                        success = false,
                        message = $"Billboard with ID {id} not found."
                    });

                var billboardDto = ConvertToDto(billboard);

                return Ok(new
                {
                    success = true,
                    data = billboardDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billboard with ID {BillboardId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the billboard."
                });
            }
        }

        // GET: api/billboards/filters/options
        [HttpGet("filters/options")]
        public async Task<IActionResult> GetFilterOptions(CancellationToken cancellationToken)
        {
            try
            {
                var options = await GetAvailableFilterOptions(cancellationToken);
                return Ok(new
                {
                    success = true,
                    data = options
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filter options");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving filter options."
                });
            }
        }

        // GET: api/billboards/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchBillboards(
            [FromQuery] string query,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest(new { success = false, message = "Search query is required." });

                var billboards = await _unitOfWork.Billboard.GetAllAsync(
                    b => (b.Code != null && b.Code.Contains(query)) ||
                         (b.Description != null && b.Description.Contains(query)) ||
                         (b.SubDescription != null && b.SubDescription.Contains(query)) ||
                         (b.Highway != null && b.Highway.Contains(query)) ||
                         (b.Type != null && b.Type.Contains(query)) ||
                         (b.Size != null && b.Size.Contains(query)) ||
                         (b.Address != null && b.Address.Name.Contains(query)),
                    includeProperties: "Address",
                    cancellationToken: cancellationToken);

                var totalCount = billboards.Count();
                var pagedBillboards = billboards
                    .OrderBy(b => b.Code)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var billboardDtos = pagedBillboards.Select(ConvertToDto).ToList();

                return Ok(new
                {
                    success = true,
                    data = billboardDtos,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    },
                    searchQuery = query
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching billboards with query: {Query}", query);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while searching billboards."
                });
            }
        }

        [HttpPost("SubmitContactForm")]
        public async Task<IActionResult> SubmitContactForm([FromBody] ContactUsDTO contactDto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid form data",
                        errors = ModelState
                    });
                }

                if (contactDto.BillboardId.HasValue)
                {
                    var billboard = await _unitOfWork.Billboard.GetAsync(
                        b => b.Id == contactDto.BillboardId.Value,
                        cancellationToken: cancellationToken);

                    if (billboard == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Billboard not found"
                        });
                    }
                }

                var contactUs = new ContactUs
                {
                    Name = contactDto.Name,
                    Email = contactDto.Email,
                    Phone = contactDto.Phone,
                    Message = contactDto.Message,
                    BillboardId = contactDto.BillboardId,
                    SubmittedAt = DateTime.UtcNow,
                    Status = "Pending"
                };

                await _unitOfWork.ContactUs.AddAsync(contactUs, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Thank you! We'll get back to you soon.",
                    data = new
                    {
                        id = contactUs.Id,
                        name = contactUs.Name,
                        email = contactUs.Email,
                        billboardId = contactUs.BillboardId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact form");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while submitting your inquiry. Please try again later."
                });
            }
        }

        #region Helper Methods

        private BillboardDTO ConvertToDto(Billboard billboard)
        {
            return new BillboardDTO
            {
                Id = billboard.Id,
                Code = billboard.Code,
                Description = billboard.Description,
                SubDescription = billboard.SubDescription,
                ImagePath = billboard.ImagePath,
                Size = billboard.Size,
                Highway = billboard.Highway,
                StartBooking = billboard.StartBooking,
                EndBooking = billboard.EndBooking,
                IsAvailable = billboard.IsAvailable ?? false,
                NumberOfFaces = billboard.NumberOfFaces,
                Type = billboard.Type,
                LocationURL = billboard.LocationURL,
                AddressId = billboard.AddressId,
                Address = billboard.Address != null ? new AddressDTO
                {
                    Id = billboard.Address.Id,
                    Name = billboard.Address.Name
                } : null
            };
        }

        private Expression<Func<Billboard, bool>> BuildFilterExpression(
            bool? availableOnly, string? location, string? highway, string? type,
            string? size, int? month, int? year, string? searchQuery)
        {
            Expression<Func<Billboard, bool>> filter = b => true;

            if (availableOnly == true)
            {
                filter = CombineFilters(filter, b =>
                    b.IsAvailable == true &&
                    b.StartBooking <= DateTime.Now &&
                    (b.EndBooking == null || b.EndBooking >= DateTime.Now));
            }

            if (!string.IsNullOrEmpty(location))
            {
                filter = CombineFilters(filter, b =>
                    b.Address != null && b.Address.Name.Contains(location));
            }

            if (!string.IsNullOrEmpty(highway))
            {
                filter = CombineFilters(filter, b =>
                    b.Highway != null && b.Highway.Contains(highway));
            }

            if (!string.IsNullOrEmpty(type))
            {
                filter = CombineFilters(filter, b =>
                    b.Type != null && b.Type.Contains(type));
            }

            if (!string.IsNullOrEmpty(size))
            {
                filter = CombineFilters(filter, b =>
                    b.Size != null && b.Size.Contains(size));
            }

            if (month.HasValue || year.HasValue)
            {
                filter = CombineFilters(filter, b =>
                    (!month.HasValue ||
                        (b.StartBooking.Month <= month.Value &&
                         (b.EndBooking == null || b.EndBooking.Value.Month >= month.Value))) &&
                    (!year.HasValue ||
                        (b.StartBooking.Year <= year.Value &&
                         (b.EndBooking == null || b.EndBooking.Value.Year >= year.Value))));
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                filter = CombineFilters(filter, b =>
                    (b.Code != null && b.Code.Contains(searchQuery)) ||
                    (b.Description != null && b.Description.Contains(searchQuery)) ||
                    (b.SubDescription != null && b.SubDescription.Contains(searchQuery)) ||
                    (b.Highway != null && b.Highway.Contains(searchQuery)) ||
                    (b.Type != null && b.Type.Contains(searchQuery)) ||
                    (b.Size != null && b.Size.Contains(searchQuery)) ||
                    (b.Address != null && b.Address.Name != null && b.Address.Name.Contains(searchQuery)));
            }

            return filter;
        }

        private Expression<Func<Billboard, bool>> CombineFilters(
            Expression<Func<Billboard, bool>> first,
            Expression<Func<Billboard, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(Billboard));
            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter));
            return Expression.Lambda<Func<Billboard, bool>>(combined, parameter);
        }

        private async Task<object> GetAvailableFilterOptions(CancellationToken cancellationToken)
        {
            var allBillboards = await _unitOfWork.Billboard.GetAllAsync(includeProperties: "Address", cancellationToken: cancellationToken);

            return new
            {
                locations = allBillboards
                    .Where(b => b.Address != null && !string.IsNullOrEmpty(b.Address.Name))
                    .Select(b => b.Address.Name)
                    .Distinct()
                    .OrderBy(name => name)
                    .ToList(),
                highways = allBillboards
                    .Where(b => !string.IsNullOrEmpty(b.Highway))
                    .Select(b => b.Highway)
                    .Distinct()
                    .OrderBy(h => h)
                    .ToList(),
                types = allBillboards
                    .Where(b => !string.IsNullOrEmpty(b.Type))
                    .Select(b => b.Type)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList(),
                sizes = allBillboards
                    .Where(b => !string.IsNullOrEmpty(b.Size))
                    .Select(b => b.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList(),
                availableMonths = Enumerable.Range(1, 12).ToList(),
                availableYears = allBillboards
                    .SelectMany(b => new[] { b.StartBooking.Year, b.EndBooking?.Year ?? b.StartBooking.Year })
                    .Distinct()
                    .OrderBy(y => y)
                    .ToList()
            };
        }

        #endregion

        public class BillboardDTO
        {
            public int Id { get; set; }
            public string? Code { get; set; }
            public string? Description { get; set; }
            public string? SubDescription { get; set; }
            public string? ImagePath { get; set; }
            public string? Size { get; set; }
            public string? Highway { get; set; }
            public DateTime StartBooking { get; set; }
            public DateTime? EndBooking { get; set; }
            public bool IsAvailable { get; set; }
            public int? NumberOfFaces { get; set; }
            public string? Type { get; set; }
            public string? LocationURL { get; set; }
            public int? AddressId { get; set; }
            public AddressDTO? Address { get; set; }
        }

        public class AddressDTO
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public class ContactUsDTO
        {
            [Required]
            public string Name { get; set; }

            [EmailAddress]
            public string? Email { get; set; }

            [Required]
            public string? Phone { get; set; }

            public string? Message { get; set; }

            public int? BillboardId { get; set; }
        }
    }
}
