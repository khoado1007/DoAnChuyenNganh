using ITManagement.Infrastructure.Persistence;
using ITManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ITManagement.IntegrationTests;

public class TicketSequenceTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Concurrent_ticket_numbers_are_unique_before_any_ticket_is_saved()
    {
        // Each operation has its own scope/DbContext, as separate HTTP requests would.
        var numbers = await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await new TicketRepository(db).GetNextSequenceAsync(2026);
        }));
        Assert.Equal(8, numbers.Distinct().Count());
    }

    [Fact]
    public async Task Commit_releases_transaction_so_another_can_start()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var work = scope.ServiceProvider.GetRequiredService<ITManagement.Domain.Interfaces.Repositories.IUnitOfWork>();
        await work.BeginTransactionAsync();
        await work.CommitTransactionAsync();
        Assert.Null(db.Database.CurrentTransaction);
        await work.BeginTransactionAsync();
        await work.RollbackTransactionAsync();
        Assert.Null(db.Database.CurrentTransaction);
    }
}
