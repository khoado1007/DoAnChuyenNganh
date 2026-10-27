using ITManagement.Domain.Entities.User;
using ITManagement.Domain.Enums;

namespace ITManagement.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<AppUser?> GetByIdentityIdAsync(string identityUserId, CancellationToken ct = default);
    Task<IEnumerable<AppUser>> GetByRoleAsync(UserRole role, CancellationToken ct = default);
    Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(AppUser user, CancellationToken ct = default);
    Task UpdateAsync(AppUser user, CancellationToken ct = default);
}
