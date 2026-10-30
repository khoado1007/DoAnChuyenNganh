using System.Text.RegularExpressions;
using ITManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
namespace ITManagement.Infrastructure.FileStorage;

// Storage keys are private, generated identifiers. Display names never become paths.
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string root;
    public LocalFileStorageService(IConfiguration config)
    {
        root = Path.GetFullPath(config["FileStorage:RootPath"] ?? throw new InvalidOperationException("FileStorage:RootPath không được cấu hình."));
        Directory.CreateDirectory(root);
        RejectLink(root);
    }
    private static void RejectLink(string path)
    {
        try
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new ArgumentException("Storage không cho phép liên kết hoặc reparse point.");
        }
        catch (FileNotFoundException) { }
        catch (DirectoryNotFoundException) { }
    }
    private string Resolve(string key)
    {
        var parts = key.Replace('\\', '/').Split('/');
        if (parts.Length != 2 || !Guid.TryParseExact(parts[0], "D", out _)
            || !Regex.IsMatch(parts[1], @"\A(?:[a-fA-F0-9]{32}|[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12})(?:\.[a-zA-Z0-9]{1,16})?\z"))
            throw new ArgumentException("Storage key không hợp lệ.");
        var directory = Path.Combine(root, parts[0]);
        var path = Path.Combine(directory, parts[1]);
        RejectLink(root); RejectLink(directory); RejectLink(path);
        return path;
    }
    public async Task<string> SaveAsync(Guid userId, string fileName, Stream content, CancellationToken ct = default)
    {
        var key = $"{userId:D}/{Guid.NewGuid():N}";
        var path = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        path = Resolve(key);
        var created = false;
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
            created = true;
            await content.CopyToAsync(output, ct);
            return key;
        }
        catch
        {
            if (created) File.Delete(path);
            throw;
        }
    }
    public Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Stream stream = new FileStream(Resolve(storagePath), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }
    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested(); File.Delete(Resolve(storagePath)); return Task.CompletedTask;
    }
    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested(); return Task.FromResult(File.Exists(Resolve(storagePath)));
    }
}
