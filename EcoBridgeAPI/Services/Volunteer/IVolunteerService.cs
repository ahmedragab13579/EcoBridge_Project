using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Result;
namespace EcoBridgeAPI.Services.Volunteer
{
    public interface IVolunteerService
    {
        Task<Result<List<VolunteerDTO>>> GetAll();

        Task<Result<VolunteerDTO>> GetById(int id);


        Task<Result<int>> Create(int accountId, CreateVolunteerDTO dto);

        Task<Result<bool>> Update(int id, CreateVolunteerDTO dto);


        Task<Result<bool>> Delete(int id);
    }
}
