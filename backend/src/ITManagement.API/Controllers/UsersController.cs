using ITManagement.API.Responses;
using ITManagement.Application.Common.Models;
using ITManagement.Application.Contracts.Users;
using ITManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ITManagement.API.Controllers;

[ApiController, Authorize(Policy = "AdminOnly")]
[Route("api/users")]
[RequestSizeLimit(8192)]
public sealed class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> List([FromQuery] UserQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<UserResponse>>.Ok(await users.ListAsync(query, ct)));
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Create(CreateUserRequest request, CancellationToken ct)
        => Ok(ApiResponse<UserResponse>.Ok(await users.CreateAsync(request, ct)));
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
        => Ok(ApiResponse<UserResponse>.Ok(await users.UpdateAsync(id, request, ct)));
}
