using EcoBridgeAPI.DTO;

namespace EcoBridgeAPI.Services.Donation
{
    public interface IDonationService
    {
        Task<Result.Result<bool>> AssignVolunteer(int id, AssignVolunteerDTO dto);

        Task<Result.Result<bool>> UpdateStatus(int id, int volunteerId, UpdateStatusDTO dto);
        Task<Result.Result<object>> GetPendingDonations();
        Task<Result.Result<bool>> AcceptDonation(int donationId, int charityId);
    }
}
