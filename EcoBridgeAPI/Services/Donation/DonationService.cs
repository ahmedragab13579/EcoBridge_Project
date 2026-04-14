using EcoBridge.Data;
using EcoBridgeAPI.Result;
using EcoBridge.Domains.Enums;
using EcoBridge.Domains.Models;
using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Services.Photo;
using Microsoft.EntityFrameworkCore;

namespace EcoBridgeAPI.Services.Donation
{
    public class DonationService : IDonationService
    {
        private readonly EcoBridgeDbContext _context;
        private readonly IPhotoService _photoService;

        public DonationService(EcoBridgeDbContext context, IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        public async Task<Result<int>> CreateDonation(int accountId, CreateDonationDTO dto, CancellationToken ct = default)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.AccountId == accountId, ct);
            if (donor == null)
                return Result<int>.FailResult(0, "Donor not found");

            string? imageUrl = null;
            if (dto.Image != null)
            {
                if (_photoService.IsEnabled)
                {
                    var uploadResult = await _photoService.UploadImageAsync(dto.Image, ct);
                    if (!uploadResult.Success)
                        return Result<int>.FailResult(0, uploadResult.Message);

                    imageUrl = uploadResult.Value;
                }
                else
                {
                    imageUrl = null;
                }
            }

            var donation = new EcoBridge.Domains.Models.Donation
            {
                DonorId = donor.AccountId,
                FoodType = dto.FoodType,
                Quantity = dto.Quantity,
                ExpiryDate = dto.ExpiryDate,
                PickupLocation = dto.PickupLocation,
                ImageUrl = imageUrl,
                Status = DonationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync(ct);

            return Result<int>.SuccessResult(donation.Id, "Donation created successfully");
        }
        public async Task<Result<bool>> UpdateDonation(int donationId, int accountId, UpdateDonationDTO dto, CancellationToken ct = default)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.AccountId == accountId, ct);
            if (donor == null)
                return Result<bool>.FailResult(false, "Donor not found");

            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == donationId, ct);
            if (donation == null)
                return Result<bool>.FailResult(false, "Donation not found");

            if (donation.DonorId != donor.AccountId)
                return Result<bool>.FailResult(false, "You can only update your own donations");

            if (donation.Status != DonationStatus.Pending)
                return Result<bool>.FailResult(false, "Only pending donations can be updated");

            if (!string.IsNullOrWhiteSpace(dto.FoodType))
                donation.FoodType = dto.FoodType;

            if (!string.IsNullOrWhiteSpace(dto.Quantity))
                donation.Quantity = dto.Quantity;

            if (dto.ExpiryDate.HasValue)
                donation.ExpiryDate = dto.ExpiryDate.Value;

            if (!string.IsNullOrWhiteSpace(dto.PickupLocation))
                donation.PickupLocation = dto.PickupLocation;

            if (dto.Image != null)
            {
                if (_photoService.IsEnabled)
                {
                    var uploadResult = await _photoService.UploadImageAsync(dto.Image, ct);
                    if (!uploadResult.Success)
                        return Result<bool>.FailResult(false, uploadResult.Message);

                    donation.ImageUrl = uploadResult.Value;
                }
                // if photo service not enabled, ignore provided image
            }

            await _context.SaveChangesAsync(ct);
            return Result<bool>.SuccessResult(true, "Donation updated successfully");
        }
        public async Task<Result<bool>> DeleteDonation(int donationId, int accountId, CancellationToken ct = default)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.AccountId == accountId, ct);
            if (donor == null)
                return Result<bool>.FailResult(false, "Donor not found");

            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == donationId, ct);
            if (donation == null)
                return Result<bool>.FailResult(false, "Donation not found");

            if (donation.DonorId != donor.AccountId)
                return Result<bool>.FailResult(false, "You can only delete your own donations");

            if (donation.Status != DonationStatus.Pending)
                return Result<bool>.FailResult(false, "Only pending donations can be deleted");

            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync(ct);
            return Result<bool>.SuccessResult(true, "Donation deleted successfully");
        }

        // =========================
        // Assign Volunteer (Admin)
        // =========================
        public async Task<Result<bool>> AssignVolunteer(int id, AssignVolunteerDTO dto, CancellationToken ct = default)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == id, ct);

            if (donation == null)
                return Result<bool>.FailResult(false, "Donation not found");

            if (donation.Status != DonationStatus.Accepted)
                return Result<bool>.FailResult(false, "Donation must be accepted first");

            if (donation.VolunteerId != null)
                return Result<bool>.FailResult(false, "Volunteer already assigned");

            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.AccountId == dto.VolunteerId, ct);

            if (volunteer == null)
                return Result<bool>.FailResult(false, "Volunteer not found");

            donation.VolunteerId = dto.VolunteerId;

            await _context.SaveChangesAsync(ct);

            return Result<bool>.SuccessResult(true, "Volunteer assigned successfully");
        }

        // =========================
        // Update Status (Volunteer)
        // =========================
        public async Task<Result<bool>> UpdateStatus(int id, int volunteerId, UpdateStatusDTO dto, CancellationToken ct = default)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == id, ct);

            if (donation == null)
                return Result<bool>.FailResult(false, "Donation not found");

            if (donation.VolunteerId != volunteerId)
                return Result<bool>.FailResult(false, "You can only update your assigned donations");

            if (!IsValidStatusTransition(donation.Status, dto.Status))
                return Result<bool>.FailResult(false, "Invalid status transition");

            donation.Status = dto.Status;

            await _context.SaveChangesAsync(ct);

            return Result<bool>.SuccessResult(true, "Status updated successfully");
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
        public async Task<Result<List<EcoBridgeAPI.DTO.DonationResponseDTO>>> GetPendingDonations(CancellationToken ct = default)
        {
            var donations = await _context.Donations
                .Where(d => d.Status == DonationStatus.Pending && d.CharityId == null)
                .Include(d => d.Donor).ThenInclude(x => x.Account)
                .Select(d => new EcoBridgeAPI.DTO.DonationResponseDTO
                {
                    Id = d.Id,
                    FoodType = d.FoodType,
                    Quantity = d.Quantity,
                    ExpiryDate = d.ExpiryDate,
                    PickupLocation = d.PickupLocation,
                    ImageUrl = d.ImageUrl,
                    Status = d.Status.ToString(),
                    DonorId = d.DonorId,
                    DonorName = d.Donor.Account.FullName,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync(ct);

            return Result<List<EcoBridgeAPI.DTO.DonationResponseDTO>>.SuccessResult(donations, "Pending donations retrieved successfully");
        }

        // =========================
        // Charity: Accept Donation
        // =========================
        public async Task<Result<bool>> AcceptDonation(int donationId, int charityId, CancellationToken ct = default)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == donationId, ct);

            if (donation == null)
                return Result<bool>.FailResult(false, "Donation not found");

            var charity = await _context.Charities
                .FirstOrDefaultAsync(c => c.AccountId == charityId, ct);

            if (charity == null)
                return Result<bool>.FailResult(false, "Charity not found");

            if (donation.Status != DonationStatus.Pending)
                return Result<bool>.FailResult(false, "Donation is already accepted or processed");

            if (donation.CharityId != null)
                return Result<bool>.FailResult(false, "Donation already taken by another charity");

            donation.Status = DonationStatus.Accepted;
            donation.CharityId = charityId;

            await _context.SaveChangesAsync(ct);

            return Result<bool>.SuccessResult(true, "Donation accepted successfully");
        }
    }
}
