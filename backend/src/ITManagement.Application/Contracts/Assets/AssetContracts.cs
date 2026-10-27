using System.ComponentModel.DataAnnotations;
using ITManagement.Domain.Enums;

namespace ITManagement.Application.Contracts.Assets;

public sealed record CreateAssetRequest(
    [Required, StringLength(50)] string AssetCode,
    [Required, StringLength(200)] string Name,
    [EnumDataType(typeof(AssetType))] AssetType Type,
    Guid? AssignedUserId);

public sealed record AssetResponse(Guid Id, string AssetCode, string Name, AssetType Type,
    AssetStatus Status, Guid? AssignedUserId, string? AssignedUserName, DateTime CreatedAt, DateTime? UpdatedAt);
