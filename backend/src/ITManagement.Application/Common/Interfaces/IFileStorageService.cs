namespace ITManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction cho service lưu trữ tệp thực tế trên Docker volume.
/// Infrastructure layer sẽ implement.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lưu file lên volume, trả về đường dẫn nội bộ (StoragePath).
    /// </summary>
    Task<string> SaveAsync(Guid userId, string fileName, Stream content, CancellationToken ct = default);

    /// <summary>
    /// Đọc nội dung file từ StoragePath (private path).
    /// </summary>
    Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Xóa file khỏi volume.
    /// </summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra file có tồn tại không.
    /// </summary>
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default);
}
