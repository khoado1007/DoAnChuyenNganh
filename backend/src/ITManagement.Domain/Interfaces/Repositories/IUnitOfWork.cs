namespace ITManagement.Domain.Interfaces.Repositories;

/// <summary>
/// Unit of Work pattern — commit toàn bộ changes trong một transaction.
/// </summary>
public interface IUnitOfWork
{
    IAssetRepository Assets { get; }
    IUserRepository Users { get; }
    ITicketRepository Tickets { get; }
    IStorageRepository Storage { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
