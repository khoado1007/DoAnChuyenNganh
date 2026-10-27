using ITManagement.Domain.Entities.User;
using ITManagement.Domain.Enums;
using ITManagement.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ITManagement.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.AppUsers.Include(u => u.StorageQuota).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => _db.AppUsers.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<AppUser?> GetByIdentityIdAsync(string identityUserId, CancellationToken ct = default)
        => _db.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId, ct);

    public async Task<IEnumerable<AppUser>> GetByRoleAsync(UserRole role, CancellationToken ct = default)
        => await _db.AppUsers.Where(u => u.Role == role).ToListAsync(ct);

    public async Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken ct = default)
        => await _db.AppUsers.ToListAsync(ct);

    public async Task AddAsync(AppUser user, CancellationToken ct = default)
        => await _db.AppUsers.AddAsync(user, ct);

    public Task UpdateAsync(AppUser user, CancellationToken ct = default)
    {
        _db.AppUsers.Update(user);
        return Task.CompletedTask;
    }
}
