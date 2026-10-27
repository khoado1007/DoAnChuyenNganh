using ITManagement.Domain.Entities.Asset;
using ITManagement.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ITManagement.Infrastructure.Persistence.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _db;

    public AssetRepository(AppDbContext db) => _db = db;

    public Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Assets.Include(a => a.Assignments).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Asset?> GetByCodeAsync(string assetCode, CancellationToken ct = default)
        => _db.Assets.FirstOrDefaultAsync(a => a.AssetCode == assetCode, ct);

    public async Task<IEnumerable<Asset>> GetAllAsync(CancellationToken ct = default)
        => await _db.Assets.ToListAsync(ct);

    public async Task<IEnumerable<Asset>> GetAvailableAsync(CancellationToken ct = default)
        => await _db.Assets.Where(a => a.Status == Domain.Enums.AssetStatus.Available).ToListAsync(ct);

    public async Task AddAsync(Asset asset, CancellationToken ct = default)
        => await _db.Assets.AddAsync(asset, ct);

    public Task UpdateAsync(Asset asset, CancellationToken ct = default)
    {
        _db.Assets.Update(asset);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByCodeAsync(string assetCode, CancellationToken ct = default)
        => _db.Assets.AnyAsync(a => a.AssetCode == assetCode, ct);
}
