using EcoBridge.Data;
using EcoBridge.Domains.Models;
using EcoBridgeAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcoBridgeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerController : ControllerBase
    {
        private readonly EcoBridgeDbContext _context;

        public VolunteerController(EcoBridgeDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var volunteers = await _context.Volunteers
                .Include(v => v.Account)
                .Select(v => new VolunteerDTO
                {
                    AccountId = v.AccountId,
                    FullName = v.Account.FullName,
                    VehicleDetails = v.VehicleDetails
                })
                .ToListAsync();

            return Ok(volunteers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var v = await _context.Volunteers
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.AccountId == id);

            if (v == null)
                return NotFound();

            return Ok(new VolunteerDTO
            {
                AccountId = v.AccountId,
                FullName = v.Account.FullName,
                VehicleDetails = v.VehicleDetails
            });
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateVolunteerDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var userId = int.Parse(userIdClaim.Value);

            var volunteer = new Volunteer
            {
                AccountId = userId,
                VehicleDetails = dto.VehicleDetails
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            return Ok(volunteer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateVolunteerDTO dto)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);

            if (volunteer == null)
                return NotFound();

            volunteer.VehicleDetails = dto.VehicleDetails;

            await _context.SaveChangesAsync();

            return Ok(volunteer);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);

            if (volunteer == null)
                return NotFound();

            _context.Volunteers.Remove(volunteer);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
