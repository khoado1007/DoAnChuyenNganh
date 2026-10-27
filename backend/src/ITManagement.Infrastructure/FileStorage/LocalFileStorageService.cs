using ITManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ITManagement.Infrastructure.FileStorage;

/// <summary>
/// Triển khai lưu file trên Docker volume mount (local disk path).
/// File được lưu trong: {StorageRoot}/{userId}/{guid}{ext}
/// StorageRoot được mount vào container qua Docker volume.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public LocalFileStorageService(IConfiguration config)
    {
        _storageRoot = config["FileStorage:RootPath"]
            ?? throw new InvalidOperationException("FileStorage:RootPath không được cấu hình.");

        if (!Directory.Exists(_storageRoot))
            Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveAsync(Guid userId, string fileName, Stream content, CancellationToken ct = default)
    {
        var userDir = Path.Combine(_storageRoot, userId.ToString());
        Directory.CreateDirectory(userDir);

        var ext = Path.GetExtension(fileName);
        var storageName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(userDir, storageName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct);

        // Trả về đường dẫn tương đối so với StorageRoot để không expose absolute path
        return Path.Combine(userId.ToString(), storageName);
    }

    public Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storageRoot, storagePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File không tồn tại trên storage.", storagePath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storageRoot, storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_storageRoot, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }
}
