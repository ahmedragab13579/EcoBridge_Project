using EcoBridgeAPI.DTO;
using EcoBridgeAPI.Result;

namespace EcoBridgeAPI.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<Result<AdminStatsDTO>> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    }
}
