using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Result;

namespace EcoBridgeAPI.Services.Donation
{
    public interface IDonationService
    {
        Task<Result<int>> CreateDonation(int accountId, CreateDonationDTO dto, CancellationToken ct = default);
        Task<Result<bool>> UpdateDonation(int donationId, int accountId, UpdateDonationDTO dto, CancellationToken ct = default);
        Task<Result<bool>> DeleteDonation(int donationId, int accountId, CancellationToken ct = default);
        Task<Result<bool>> AssignVolunteer(int id, AssignVolunteerDTO dto, CancellationToken ct = default);

        Task<Result<bool>> UpdateStatus(int id, int volunteerId, UpdateStatusDTO dto, CancellationToken ct = default);
        Task<Result<List<EcoBridgeAPI.DTO.DonationResponseDTO>>> GetPendingDonations(CancellationToken ct = default);
        Task<Result<bool>> AcceptDonation(int donationId, int charityId, CancellationToken ct = default);
    }
}
