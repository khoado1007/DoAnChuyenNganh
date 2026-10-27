using ITManagement.Application.Contracts.Auth;
using ITManagement.Application.Contracts.Users;

namespace ITManagement.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserResponse> GetCurrentUserAsync(CancellationToken ct = default);
}
