using ITManagement.API.Responses;
using ITManagement.Application.Common.Models;
using ITManagement.Application.Contracts.Files;
using ITManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ITManagement.API.Controllers;
[ApiController, Authorize(Roles = "Employee"), Route("api/files")]
public sealed class FilesController(IFileService files) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FileUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileUploadLimits.MaxRequestBytes)]
    [ProducesResponseType(typeof(ApiResponse<FileResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status413RequestEntityTooLarge)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (Request.Form.Files.Count != 1)
            return BadRequest(ApiResponse.Fail("Chỉ tải lên một file mỗi yêu cầu."));
        await using var content = file.OpenReadStream();
        var result = await files.UploadAsync(file.FileName, content, ct);
        return CreatedAtAction(nameof(Download), new { id = result.Id }, ApiResponse<FileResponse>.Ok(result));
    }
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FileResponse>>>> List([FromQuery] FileQuery query, CancellationToken ct) =>
        Ok(ApiResponse<PagedResult<FileResponse>>.Ok(await files.ListAsync(query, ct)));
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await files.DownloadAsync(id, ct);
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, "application/octet-stream", file.FileName);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await files.DeleteAsync(id, ct); return NoContent();
    }
}
