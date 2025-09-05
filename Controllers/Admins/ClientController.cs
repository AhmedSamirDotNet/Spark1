using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;

namespace Spark.WebApi.Controllers.Admins
{
    [Route("admin/api/[controller]")]
    [Authorize]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: admin/api/client/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _unitOfWork.Client.GetAllAsync();
            return Ok(clients);
        }

        // GET: admin/api/client/5
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var client = await _unitOfWork.Client.GetAsync(c => c.Id == id);
            if (client == null)
                return NotFound(new { message = "Client not found" });

            return Ok(client);
        }

        // POST: admin/api/client/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            client.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Client.AddAsync(client);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
        }

        // PUT: admin/api/client/5
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Client client)
        {
            if (id != client.Id)
                return BadRequest(new { message = "Client ID mismatch" });

            var clientFromDb = await _unitOfWork.Client.GetAsync(c => c.Id == id);
            if (clientFromDb == null)
                return NotFound(new { message = "Client not found" });

            // Update fields manually (exclude Bookings)
            clientFromDb.Name = client.Name;
            clientFromDb.LogoPath = client.LogoPath;

            await _unitOfWork.Client.UpdateAsync(clientFromDb);

            return Ok(clientFromDb);
        }

        // DELETE: admin/api/client/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _unitOfWork.Client.GetAsync(c => c.Id == id);
            if (client == null)
                return NotFound(new { message = "Client not found" });

            _unitOfWork.Client.RemoveAsync(client);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Client deleted successfully" });
        }
    }
}
