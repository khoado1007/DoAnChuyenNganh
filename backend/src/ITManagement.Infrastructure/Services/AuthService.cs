using ITManagement.Application.Common.Exceptions;
using ITManagement.Application.Common.Interfaces;
using ITManagement.Application.Contracts.Auth;
using ITManagement.Application.Contracts.Users;
using ITManagement.Application.Services;
using ITManagement.Domain.Enums;
using ITManagement.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace ITManagement.Infrastructure.Services;

public sealed class AuthService(UserManager<IdentityUser> userManager, IUserRepository users,
    IJwtService jwt, ICurrentUserService currentUser) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var identity = await userManager.FindByNameAsync(request.Username.Trim());
        if (identity is null || !await userManager.CheckPasswordAsync(identity, request.Password)
            || await userManager.IsLockedOutAsync(identity))
            throw InvalidCredentials();

        var user = await users.GetByIdentityIdAsync(identity.Id, ct);
        if (user is null || user.Status != UserStatus.Active) throw InvalidCredentials();

        var token = jwt.GenerateAccessToken(user.Id, user.Username, user.Role.ToString(), user.Email);
        return new AuthResponse(token, UserResponse.From(user));
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is not Guid id)
            throw new AuthenticationRequiredException("Vui lòng đăng nhập.");
        var user = await users.GetByIdAsync(id, ct);
        if (user is null || user.Status != UserStatus.Active)
            throw new AuthenticationRequiredException("Phiên đăng nhập không còn hợp lệ.");
        return UserResponse.From(user);
    }

    private static AuthenticationRequiredException InvalidCredentials()
        => new("Tên đăng nhập hoặc mật khẩu không hợp lệ.");
}
