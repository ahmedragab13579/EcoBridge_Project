using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Services.Volunteer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridgeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerService _services;

        public VolunteerController(IVolunteerService services)
        {
            _services = services;
        }


        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            var result = await _services.GetAll();
            if (result._success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _services.GetById(id);
            if (result._success)
                return Ok(result);
            return NotFound(result);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateVolunteerDTO dto)
        {
            var result = await _services.Create(dto.AccountId, dto);
            if (result._success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateVolunteerDTO dto)
        {
            var result = await _services.Update(id, dto);
            if (result._success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _services.Delete(id);
            if (result._success)
                return Ok(result);
            return NotFound(result);
        }
    }
}
