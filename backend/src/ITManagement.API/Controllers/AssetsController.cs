using ITManagement.API.Responses;
using ITManagement.Application.Common.Models;
using ITManagement.Application.Contracts.Assets;
using ITManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ITManagement.API.Controllers;

[ApiController, Authorize(Policy = "AdminOnly")]
[Route("api/assets")]
[RequestSizeLimit(8192)]
public sealed class AssetsController(IAssetService assets) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AssetLookupResponse>>>> List(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<AssetLookupResponse>>.Ok(await assets.LookupAsync(ct)));
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<AssetResponse>>>> Search([FromQuery] AssetQuery query, CancellationToken ct)
        => Ok(ApiResponse<PagedResult<AssetResponse>>.Ok(await assets.SearchAsync(query, ct)));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AssetResponse>>> Get(Guid id, CancellationToken ct)
        => Ok(ApiResponse<AssetResponse>.Ok(await assets.GetAsync(id, ct)));
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssetResponse>>> Create(CreateAssetRequest request, CancellationToken ct)
        => Ok(ApiResponse<AssetResponse>.Ok(await assets.CreateAsync(request, ct)));
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AssetResponse>>> Update(Guid id, UpdateAssetRequest request, CancellationToken ct)
        => Ok(ApiResponse<AssetResponse>.Ok(await assets.UpdateAsync(id, request, ct)));
}
