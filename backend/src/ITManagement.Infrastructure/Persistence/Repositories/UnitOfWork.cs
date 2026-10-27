using ITManagement.Domain.Interfaces.Repositories;

namespace ITManagement.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public IAssetRepository Assets { get; }
    public IUserRepository Users { get; }
    public ITicketRepository Tickets { get; }
    public IStorageRepository Storage { get; }

    public UnitOfWork(
        AppDbContext db,
        IAssetRepository assets,
        IUserRepository users,
        ITicketRepository tickets,
        IStorageRepository storage)
    {
        _db = db;
        Assets = assets;
        Users = users;
        Tickets = tickets;
        Storage = storage;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        var transaction = _db.Database.CurrentTransaction;
        if (transaction is null) return;
        await transaction.CommitAsync(ct);
        await transaction.DisposeAsync();
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        var transaction = _db.Database.CurrentTransaction;
        if (transaction is null) return;
        try { await transaction.RollbackAsync(ct); }
        finally { await transaction.DisposeAsync(); }
    }
}
