using EcoBridgeAPI.DTO;
namespace EcoBridgeAPI.Services.Volunteer
{
    public interface IVolunteerService
    {
        Task<Result.Result<List<VolunteerDTO>>> GetAll();

        Task<Result.Result<VolunteerDTO>> GetById(int id);


        Task<Result.Result<int>> Create(int accountId, CreateVolunteerDTO dto);

        Task<Result.Result<bool>> Update(int id, CreateVolunteerDTO dto);


        Task<Result.Result<bool>> Delete(int id);
    }
}
