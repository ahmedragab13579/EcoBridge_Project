using EcoBridgeAPI.DTO;

namespace EcoBridgeAPI.Services.Donation
{
    public interface IDonationServices
    {
        Task<Result.Result<bool>> AssignVolunteer(int id, AssignVolunteerDTO dto);

        Task<Result.Result<bool>> UpdateStatus(int id, int volunteerId, UpdateStatusDTO dto);
    }
}
