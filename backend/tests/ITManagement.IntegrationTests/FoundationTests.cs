using ITManagement.Infrastructure.Persistence;
using ITManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ITManagement.IntegrationTests;

public class FoundationTests
{
    private static AppDbContext UnreachableDatabase() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=unreachable;Username=postgres;Password=postgres;Timeout=1")
            .Options);

    [Fact]
    public async Task Begin_transaction_propagates_database_failure()
    {
        await using var db = UnreachableDatabase();
        var work = new UnitOfWork(db, new AssetRepository(db), new UserRepository(db),
            new TicketRepository(db), new StorageRepository(db));
        await Assert.ThrowsAnyAsync<Exception>(() => work.BeginTransactionAsync());
    }

    [Fact]
    public async Task Asset_list_observes_cancellation_before_returning_data()
    {
        await using var db = UnreachableDatabase();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => new AssetRepository(db).GetAllAsync(cancellation.Token));
    }
}
