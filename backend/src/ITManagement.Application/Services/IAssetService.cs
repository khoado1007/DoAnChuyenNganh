using ITManagement.Application.Common.Models;
using ITManagement.Application.Contracts.Assets;
namespace ITManagement.Application.Services;
public interface IAssetService
{
    Task<IReadOnlyList<AssetLookupResponse>> LookupAsync(CancellationToken ct);
    Task<PagedResult<AssetResponse>> SearchAsync(AssetQuery query, CancellationToken ct);
    Task<AssetResponse> GetAsync(Guid id, CancellationToken ct);
    Task<AssetResponse> CreateAsync(CreateAssetRequest request, CancellationToken ct);
    Task<AssetResponse> UpdateAsync(Guid id, UpdateAssetRequest request, CancellationToken ct);
}
