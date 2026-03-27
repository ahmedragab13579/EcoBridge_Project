namespace EcoBridgeAPI.Services.Volunteer
{
    using EcoBridge.Data;
    using EcoBridge.Domains.Models;
    using EcoBridgeAPI.DTO;
    using Microsoft.EntityFrameworkCore;

    public class VolunteerServices : IVolunteerServices
    {
        private readonly EcoBridgeDbContext _context;

        public VolunteerServices(EcoBridgeDbContext context)
        {
            _context = context;
        }

        public async Task<Result.Result<List<VolunteerDTO>>> GetAll()
        {
            var volunteers = await _context.Volunteers
                .AsNoTracking()
                .Include(v => v.Account)
                .Select(v => new VolunteerDTO
                {
                    AccountId = v.AccountId,
                    FullName = v.Account.FullName,
                    VehicleDetails = v.VehicleDetails ?? string.Empty
                })
                .ToListAsync();

            return Result.Result<List<VolunteerDTO>>.Success(volunteers, "Volunteers fetched successfully");
        }

        public async Task<Result.Result<VolunteerDTO>> GetById(int id)
        {
            var volunteer = await _context.Volunteers
                .AsNoTracking()
                .Include(v => v.Account)
                .FirstOrDefaultAsync(v => v.AccountId == id);

            if (volunteer == null)
                return Result.Result<VolunteerDTO>.Fail(null!, "Volunteer not found");

            var dto = new VolunteerDTO
            {
                AccountId = volunteer.AccountId,
                FullName = volunteer.Account.FullName,
                VehicleDetails = volunteer.VehicleDetails ?? string.Empty
            };

            return Result.Result<VolunteerDTO>.Success(dto, "Volunteer fetched successfully");
        }

        public async Task<Result.Result<int>> Create(int accountId, CreateVolunteerDTO dto)
        {
            var exists = await _context.Volunteers.AnyAsync(v => v.AccountId == accountId);
            if (exists)
                return Result.Result<int>.Fail(0, "Volunteer already exists for this account");

            var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId);
            if (!accountExists)
                return Result.Result<int>.Fail(0, "Account not found");

            var volunteer = new Volunteer
            {
                AccountId = accountId,
                VehicleDetails = dto.VehicleDetails
            };

            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            return Result.Result<int>.Success(volunteer.AccountId, "Volunteer created successfully");
        }

        public async Task<Result.Result<bool>> Update(int id, CreateVolunteerDTO dto)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer == null)
                return Result.Result<bool>.Fail(false, "Volunteer not found");

            volunteer.VehicleDetails = dto.VehicleDetails;
            await _context.SaveChangesAsync();

            return Result.Result<bool>.Success(true, "Volunteer updated successfully");
        }

        public async Task<Result.Result<bool>> Delete(int id)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer == null)
                return Result.Result<bool>.Fail(false, "Volunteer not found");

            _context.Volunteers.Remove(volunteer);
            await _context.SaveChangesAsync();

            return Result.Result<bool>.Success(true, "Volunteer deleted successfully");
        }
    }
}
