using ITManagement.Domain.Entities.Storage;
using ITManagement.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ITManagement.Infrastructure.Persistence.Repositories;

public class StorageRepository : IStorageRepository
{
    private readonly AppDbContext _db;

    public StorageRepository(AppDbContext db) => _db = db;

    public Task<StorageQuota?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _db.StorageQuotas.Include(q => q.Files).FirstOrDefaultAsync(q => q.UserId == userId, ct);

    public async Task<StorageQuota> GetOrCreateAsync(Guid userId, CancellationToken ct = default)
    {
        var quota = await GetByUserIdAsync(userId, ct);
        if (quota != null) return quota;

        quota = new StorageQuota { UserId = userId };
        await _db.StorageQuotas.AddAsync(quota, ct);
        return quota;
    }

    public Task<FileMetadata?> GetFileByIdAsync(Guid fileId, CancellationToken ct = default)
        => _db.FileMetadata.Include(f => f.StorageQuota).FirstOrDefaultAsync(f => f.Id == fileId, ct);

    public async Task<IEnumerable<FileMetadata>> GetFilesByUserAsync(Guid userId, string? folder = null, CancellationToken ct = default)
    {
        var query = _db.FileMetadata
            .Include(f => f.StorageQuota)
            .Where(f => f.StorageQuota.UserId == userId);

        if (!string.IsNullOrWhiteSpace(folder))
            query = query.Where(f => f.VirtualFolder == folder);

        return await query.OrderBy(f => f.DisplayName).ToListAsync(ct);
    }

    public async Task AddFileAsync(FileMetadata file, CancellationToken ct = default)
        => await _db.FileMetadata.AddAsync(file, ct);

    public Task UpdateQuotaAsync(StorageQuota quota, CancellationToken ct = default)
    {
        _db.StorageQuotas.Update(quota);
        return Task.CompletedTask;
    }

    public async Task DeleteFileAsync(Guid fileId, CancellationToken ct = default)
    {
        var file = await _db.FileMetadata.FindAsync([fileId], ct);
        if (file != null) _db.FileMetadata.Remove(file);
    }
}
