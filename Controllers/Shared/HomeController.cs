using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System.ComponentModel.DataAnnotations;

namespace Spark.WebApi.Controllers.Shared
{
    [Route("api/shared/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost("ContactUs")]
        public async Task<IActionResult> SubmitContactUsAsync([FromBody] ContactUs contact, CancellationToken cancellationToken)
        {
            if (contact == null)
                return BadRequest(new { message = "Invalid data." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _unitOfWork.ContactUs.AddAsync(contact, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                return Ok(new
                {
                    message = "Your message has been submitted successfully.",
                    data = new
                    {
                        contact.Id,
                        contact.Name,
                        contact.Email,
                        contact.Phone,
                        contact.SubmittedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while saving your message.",
                    detail = ex.Message
                });
            }
        }



        public class ContactUsDTO
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string? Email { get; set; }

            public string Phone { get; set; }

            public string? Message { get; set; }

            public int? BillboardId { get; set; } // Optional: ID of the billboard being inquired
        }

    }
}
