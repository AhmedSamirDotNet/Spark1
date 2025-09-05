using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;

namespace Spark.WebApi.Controllers.Admins
{
    [Route("api/[controller]")]
   // [ApiController]
    public class ContactUsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContactUsController> _logger;

        public ContactUsController(IUnitOfWork unitOfWork, ILogger<ContactUsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Get All with optional filter
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken)
        {
            try
            {
                var contacts = await _unitOfWork.ContactUs.GetAllAsync(
                    filter: c => string.IsNullOrEmpty(status) || c.Status == status,
                    cancellationToken: cancellationToken
                );

                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ContactUs records.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving contacts." });
            }
        }
        #endregion

        #region Get By Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var contact = await _unitOfWork.ContactUs.GetAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (contact == null)
                return NotFound(new { message = $"ContactUs with Id {id} not found." });

            return Ok(contact);
        }
        #endregion

        #region Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContactUs model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.SubmittedAt = DateTime.UtcNow;
            model.Status = "Pending";

            await _unitOfWork.ContactUs.AddAsync(model, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }
        #endregion

        #region Update
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ContactUs request, CancellationToken cancellationToken)
        {
            var contact = await _unitOfWork.ContactUs.GetAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (contact == null)
                return NotFound(new { message = $"ContactUs with Id {id} not found." });

            // Update fields
            contact.Name = request.Name ?? contact.Name;
            contact.Email = request.Email ?? contact.Email;
            contact.Phone = request.Phone ?? contact.Phone;
            contact.Message = request.Message ?? contact.Message;
            contact.BillboardId = request.BillboardId ?? contact.BillboardId;
            contact.Status = request.Status ?? contact.Status;

            await _unitOfWork.SaveAsync(cancellationToken);

            return Ok(contact);
        }
        #endregion

        #region Delete
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var contact = await _unitOfWork.ContactUs.GetAsync(c => c.Id == id, cancellationToken: cancellationToken);
            if (contact == null)
                return NotFound(new { message = $"ContactUs with Id {id} not found." });

            await _unitOfWork.ContactUs.RemoveAsync(contact, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return Ok(new { message = "Contact deleted successfully." });
        }
        #endregion
    }
}
