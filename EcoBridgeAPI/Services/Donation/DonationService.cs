using EcoBridge.Data;
using EcoBridge.Domains.Enums;
using EcoBridgeAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace EcoBridgeAPI.Services.Donation
{
    public class DonationService : IDonationService
    {
        private readonly EcoBridgeDbContext _context;

        public DonationService(EcoBridgeDbContext context)
        {
            _context = context;
        }

        // =========================
        // Assign Volunteer (Admin)
        // =========================
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

        // =========================
        // Update Status (Volunteer)
        // =========================
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

        // =========================
        // Status Validation
        // =========================
        private bool IsValidStatusTransition(DonationStatus current, DonationStatus next)
        {
            return (current == DonationStatus.Accepted && next == DonationStatus.PickedUp)
                || (current == DonationStatus.PickedUp && next == DonationStatus.Delivered);
        }

        // =========================
        // Charity: Get Pending Donations
        // =========================
        public async Task<Result.Result<object>> GetPendingDonations()
        {
            var donations = await _context.Donations
                .Where(d => d.Status == DonationStatus.Pending && d.CharityId == null)
                .ToListAsync();

            return Result.Result<object>.Success(donations, "Pending donations retrieved successfully");
        }

        // =========================
        // Charity: Accept Donation
        // =========================
        public async Task<Result.Result<bool>> AcceptDonation(int donationId, int charityId)
        {
            var donation = await _context.Donations.FindAsync(donationId);

            if (donation == null)
                return Result.Result<bool>.Fail(false, "Donation not found");

            var charity = await _context.Charities
                .FirstOrDefaultAsync(c => c.AccountId == charityId);

            if (charity == null)
                return Result.Result<bool>.Fail(false, "Charity not found");

            if (donation.Status != DonationStatus.Pending)
                return Result.Result<bool>.Fail(false, "Donation is already accepted or processed");

            if (donation.CharityId != null)
                return Result.Result<bool>.Fail(false, "Donation already taken by another charity");

            donation.Status = DonationStatus.Accepted;
            donation.CharityId = charityId;

            await _context.SaveChangesAsync();

            return Result.Result<bool>.Success(true, "Donation accepted successfully");
        }
    }
}