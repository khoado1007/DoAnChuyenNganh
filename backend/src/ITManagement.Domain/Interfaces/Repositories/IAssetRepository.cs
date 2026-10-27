using ITManagement.Domain.Entities.Asset;

namespace ITManagement.Domain.Interfaces.Repositories;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Asset?> GetByCodeAsync(string assetCode, CancellationToken ct = default);
    Task<IEnumerable<Asset>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Asset>> GetAvailableAsync(CancellationToken ct = default);
    Task AddAsync(Asset asset, CancellationToken ct = default);
    Task UpdateAsync(Asset asset, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string assetCode, CancellationToken ct = default);
}
