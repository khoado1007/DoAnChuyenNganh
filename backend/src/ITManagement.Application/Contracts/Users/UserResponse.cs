using ITManagement.Domain.Entities.User;
using ITManagement.Domain.Enums;

namespace ITManagement.Application.Contracts.Users;

public sealed record UserResponse(Guid Id, string Username, string Email, string FullName,
    string Department, UserRole Role, UserStatus Status, DateTime CreatedAt)
{
    public static UserResponse From(AppUser user) => new(user.Id, user.Username, user.Email,
        user.FullName, user.Department, user.Role, user.Status, user.CreatedAt);
}
