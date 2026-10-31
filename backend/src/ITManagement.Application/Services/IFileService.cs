using ITManagement.Application.Common.Models;
using ITManagement.Application.Contracts.Files;
namespace ITManagement.Application.Services;
public interface IFileService
{
    Task<PagedResult<FileResponse>> ListAsync(FileQuery query, CancellationToken ct);
    Task<FileDownload> DownloadAsync(Guid id, CancellationToken ct);
    Task<FileResponse> UploadAsync(string fileName, Stream content, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
