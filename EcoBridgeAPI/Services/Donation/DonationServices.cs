using EcoBridge.Data;
using EcoBridge.Domains.Enums;
using EcoBridgeAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace EcoBridgeAPI.Services.Donation
{
    public class DonationServices : IDonationServices
    {
        private readonly EcoBridgeDbContext _context;

        public DonationServices(EcoBridgeDbContext context)
        {
            _context = context;
        }

        public async Task<Result.Result<bool>> AssignVolunteer(int id, AssignVolunteerDTO dto)
        {
            var donation = await _context.Donations.FindAsync(id);

            if (donation == null)
                return Result.Result<bool>.Fail(false, "Donation not found");

            if (donation.Status != DonationStatus.Accepted)
                return Result.Result<bool>.Fail(false, "Donation must be accepted first");

            if (donation.VolunteerId != null)
                return Result.Result<bool>.Fail(false, "Volunteer already assigned");

            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.AccountId == dto.VolunteerId);

            if (volunteer == null)
                return Result.Result<bool>.Fail(false, "Volunteer not found");

            donation.VolunteerId = dto.VolunteerId;

            await _context.SaveChangesAsync();

            return Result.Result<bool>.Success(true, "Volunteer assigned successfully");
        }

        public async Task<Result.Result<bool>> UpdateStatus(int id, int volunteerId, UpdateStatusDTO dto)
        {
            var donation = await _context.Donations.FindAsync(id);

            if (donation == null)
                return Result.Result<bool>.Fail(false, "Donation not found");

            if (donation.VolunteerId != volunteerId)
                return Result.Result<bool>.Fail(false, "You can only update your assigned donations");

            if (!IsValidStatusTransition(donation.Status, dto.Status))
                return Result.Result<bool>.Fail(false, "Invalid status transition");

            donation.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Result.Result<bool>.Success(true, "Status updated successfully");
        }

        private bool IsValidStatusTransition(DonationStatus current, DonationStatus next)
        {
            return (current == DonationStatus.Accepted && next == DonationStatus.PickedUp)
                || (current == DonationStatus.PickedUp && next == DonationStatus.Delivered);
        }

    }
}
