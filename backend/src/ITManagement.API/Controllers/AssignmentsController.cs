using ITManagement.API.Responses;
using ITManagement.Application.Contracts.Assets;
using ITManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ITManagement.API.Controllers;
[ApiController, Authorize(Policy = "AdminOnly"), Route("api/assets/{assetId:guid}"), RequestSizeLimit(8192)]
public sealed class AssignmentsController(IAssignmentService assignments) : ControllerBase
{
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(Guid assetId, AssignAssetRequest request, CancellationToken ct) =>
        Ok(ApiResponse<AssignmentResponse>.Ok(await assignments.AssignAsync(assetId, request, ct)));
    [HttpPost("return")]
    public async Task<IActionResult> Return(Guid assetId, CloseAssignmentRequest request, CancellationToken ct) =>
        Ok(ApiResponse<AssignmentResponse>.Ok(await assignments.CloseAsync(assetId, request, false, ct)));
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(Guid assetId, CloseAssignmentRequest request, CancellationToken ct) =>
        Ok(ApiResponse<AssignmentResponse>.Ok(await assignments.CloseAsync(assetId, request, true, ct)));
    [HttpGet("assignments")]
    public async Task<IActionResult> History(Guid assetId, CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<AssignmentResponse>>.Ok(await assignments.HistoryAsync(assetId, ct)));
}
[ApiController, Authorize(Roles = "Employee"), Route("api/me/assets")]
public sealed class MyAssetsController(IAssignmentService assignments) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<AssignmentResponse>>.Ok(await assignments.MineAsync(ct)));
}
