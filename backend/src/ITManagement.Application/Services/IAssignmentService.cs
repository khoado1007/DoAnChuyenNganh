using ITManagement.Application.Contracts.Assets;
namespace ITManagement.Application.Services;
public interface IAssignmentService
{
    Task<AssignmentResponse> AssignAsync(Guid assetId, AssignAssetRequest request, CancellationToken ct);
    Task<AssignmentResponse> CloseAsync(Guid assetId, CloseAssignmentRequest request, bool revoke, CancellationToken ct);
    Task<IReadOnlyList<AssignmentResponse>> HistoryAsync(Guid assetId, CancellationToken ct);
    Task<IReadOnlyList<AssignmentResponse>> MineAsync(CancellationToken ct);
}
