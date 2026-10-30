using System.ComponentModel.DataAnnotations;
namespace ITManagement.Application.Contracts.Files;
public sealed class FileQuery
{
    [Range(1, 100000)] public int Page { get; init; } = 1;
    [Range(1, 100)] public int PageSize { get; init; } = 20;
}
public sealed record FileResponse(Guid Id, string DisplayName, long SizeBytes, DateTime UploadedAt, string VirtualFolder);
public sealed record FileDownload(Stream Content, string FileName);
