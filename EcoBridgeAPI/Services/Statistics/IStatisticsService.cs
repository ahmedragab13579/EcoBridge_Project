using EcoBridge.DTO;

namespace EcoBridgeAPI.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<Result.Result<AdminStatsDTO>> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    }
}
