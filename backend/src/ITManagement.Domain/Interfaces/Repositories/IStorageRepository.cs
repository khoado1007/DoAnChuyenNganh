using ITManagement.Domain.Entities.Storage;

namespace ITManagement.Domain.Interfaces.Repositories;

public interface IStorageRepository
{
    Task<StorageQuota?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<StorageQuota> GetOrCreateAsync(Guid userId, CancellationToken ct = default);
    Task<FileMetadata?> GetFileByIdAsync(Guid fileId, CancellationToken ct = default);
    Task<IEnumerable<FileMetadata>> GetFilesByUserAsync(Guid userId, string? folder = null, CancellationToken ct = default);
    Task AddFileAsync(FileMetadata file, CancellationToken ct = default);
    Task UpdateQuotaAsync(StorageQuota quota, CancellationToken ct = default);
    Task DeleteFileAsync(Guid fileId, CancellationToken ct = default);
}
