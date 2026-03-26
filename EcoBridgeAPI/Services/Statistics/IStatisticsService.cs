using EcoBridge.DTO;

namespace EcoBridgeAPI.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<AdminStatsDTO> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    }
}
