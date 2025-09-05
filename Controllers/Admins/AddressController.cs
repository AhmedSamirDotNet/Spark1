using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark.DataAccess.Repository.IRepository;
using Spark.Models;
using System;
using System.Threading.Tasks;

namespace Spark.WebApi.Controllers.Admins
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/admin/address/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var addresses = await _unitOfWork.Address.GetAllAsync();
            return Ok(addresses);
        }

        // GET: api/admin/address/GetBy/2
        [HttpGet("GetBy/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var address = await _unitOfWork.Address.GetAsync(a => a.Id == id);
            if (address == null)
                return NotFound(new { message = "Address not found" });

            return Ok(address);
        }

        // POST: api/admin/address/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Address address)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _unitOfWork.Address.AddAsync(address);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(Get), new { id = address.Id }, address);
        }

        // PUT: api/admin/address/Update/5
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Address address)
        {
            if (id != address.Id)
                return BadRequest(new { message = "Address ID mismatch" });

            var addressFromDb = await _unitOfWork.Address.GetAsync(a => a.Id == id);
            if (addressFromDb == null)
                return NotFound(new { message = "Address not found" });

            // Update fields
            addressFromDb.Name = address.Name;

            await _unitOfWork.Address.UpdateAsync(addressFromDb);
            await _unitOfWork.SaveAsync();

            return Ok(addressFromDb);
        }

        // DELETE: api/admin/address/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var address = await _unitOfWork.Address.GetAsync(a => a.Id == id);
            if (address == null)
                return NotFound(new { message = "Address not found" });

            await _unitOfWork.Address.RemoveAsync(address);
            await _unitOfWork.SaveAsync();

            return Ok(new { message = "Address deleted successfully" });
        }
    }
}
