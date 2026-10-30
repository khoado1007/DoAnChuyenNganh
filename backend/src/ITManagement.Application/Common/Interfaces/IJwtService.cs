namespace ITManagement.Application.Common.Interfaces;

/// <summary>
/// JWT token generation service.
/// </summary>
public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string username, string role, string email, int securityVersion = 0);
    string GenerateRefreshToken();
    (Guid userId, string username, string role) ValidateToken(string token);
}
