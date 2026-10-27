using ITManagement.API.Responses;
using ITManagement.Application.Contracts.Auth;
using ITManagement.Application.Contracts.Users;
using ITManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken ct)
        => Ok(ApiResponse<AuthResponse>.Ok(await auth.LoginAsync(request, ct)));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Me(CancellationToken ct)
        => Ok(ApiResponse<UserResponse>.Ok(await auth.GetCurrentUserAsync(ct)));
}
