using ITManagement.Application.Common.Interfaces;
using ITManagement.Application.Common.Models;
using ITManagement.Application.Contracts.Files;
using ITManagement.Application.Services;
using ITManagement.Domain.Entities.Storage;
using ITManagement.Domain.Exceptions;
using ITManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace ITManagement.Infrastructure.Services;

public sealed class FileService(AppDbContext db, IFileStorageService storage, ICurrentUserService current) : IFileService
{
    private IQueryable<FileMetadata> Owned => db.FileMetadata.Where(f => f.StorageQuota.UserId == current.UserId && !f.StorageQuota.IsDeleted);
    private void CheckOwnerKey(string key)
    {
        var parts = key.Replace('\\', '/').Split('/');
        if (parts.Length != 2 || !Guid.TryParseExact(parts[0], "D", out var owner) || owner != current.UserId)
            throw new BusinessRuleException("Metadata file không hợp lệ; cần quản trị kiểm tra.");
    }
    public async Task<PagedResult<FileResponse>> ListAsync(FileQuery query, CancellationToken ct)
    {
        var files = Owned.AsNoTracking().Where(f => !f.IsDeleted);
        var count = await files.CountAsync(ct);
        var rows = await files.OrderByDescending(f => f.UploadedAt).ThenBy(f => f.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(f => new FileResponse(f.Id, f.DisplayName, f.SizeBytes, f.UploadedAt, f.VirtualFolder)).ToListAsync(ct);
        return PagedResult<FileResponse>.Create(rows, query.Page, query.PageSize, count);
    }
    public async Task<FileDownload> DownloadAsync(Guid id, CancellationToken ct)
    {
        var file = await Owned.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id && !f.IsDeleted, ct) ?? throw new NotFoundException("File", id);
        CheckOwnerKey(file.StoragePath);
        var name = file.DisplayName.Replace('\\', '/').Split('/').Last();
        name = string.Concat(name.Where(c => !char.IsControl(c)));
        if (string.IsNullOrWhiteSpace(name)) name = "download";
        try { return new(await storage.ReadAsync(file.StoragePath, ct), name); }
        catch (FileNotFoundException) { throw new NotFoundException("File", id); }
        catch (DirectoryNotFoundException) { throw new NotFoundException("File", id); }
        catch (ArgumentException) { throw new BusinessRuleException("Metadata file không hợp lệ; cần quản trị kiểm tra."); }
        catch (IOException) { throw new BusinessRuleException("Không đọc được file. Vui lòng thử lại."); }
        catch (UnauthorizedAccessException) { throw new BusinessRuleException("Không đọc được file. Vui lòng thử lại."); }
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        string key;
        await using (var tx = await db.Database.BeginTransactionAsync(ct))
        {
            // All quota mutations must use this row lock (including future uploads).
            var quota = await db.StorageQuotas.FromSqlInterpolated($"SELECT * FROM \"StorageQuotas\" WHERE \"UserId\" = {current.UserId} FOR UPDATE")
                .SingleOrDefaultAsync(q => !q.IsDeleted, ct) ?? throw new NotFoundException("File", id);
            var file = await db.FileMetadata.SingleOrDefaultAsync(f => f.Id == id && f.StorageQuotaId == quota.Id, ct)
                ?? throw new NotFoundException("File", id);
            key = file.StoragePath; CheckOwnerKey(key);
            try { await storage.ExistsAsync(key, ct); } // Validate private key before changing accounting.
            catch (ArgumentException) { throw new BusinessRuleException("Metadata file không hợp lệ; cần quản trị kiểm tra."); }
            catch (IOException) { throw new BusinessRuleException("Không kiểm tra được storage. Vui lòng thử lại."); }
            catch (UnauthorizedAccessException) { throw new BusinessRuleException("Không kiểm tra được storage. Vui lòng thử lại."); }
            if (!file.IsDeleted)
            {
                if (file.SizeBytes < 0 || quota.UsedBytes < file.SizeBytes)
                    throw new BusinessRuleException("Dung lượng lưu trữ không nhất quán; cần quản trị kiểm tra.");
                file.IsDeleted = true; file.UpdatedAt = DateTime.UtcNow; file.UpdatedBy = current.Username;
                quota.UsedBytes -= file.SizeBytes;
                await db.SaveChangesAsync(ct);
            }
            await tx.CommitAsync(ct);
        }
        // Keep tombstone/key: failure or crash here is recoverable by retrying DELETE.
        try { await storage.DeleteAsync(key, ct); }
        catch (IOException) { throw new BusinessRuleException("File đã ẩn; hãy thử xóa lại để hoàn tất dọn storage."); }
        catch (UnauthorizedAccessException) { throw new BusinessRuleException("File đã ẩn; hãy thử xóa lại để hoàn tất dọn storage."); }
        catch (ArgumentException) { throw new BusinessRuleException("Metadata file không hợp lệ; cần quản trị kiểm tra."); }
    }
}

