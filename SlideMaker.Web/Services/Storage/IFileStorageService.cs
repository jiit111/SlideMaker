using SlideMaker.Web.Models;

namespace SlideMaker.Web.Services.Storage;

public class FileSaveResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StoredFileName { get; set; }
    public string? StoragePath { get; set; }
    public string? RelativeUrl { get; set; }
    public long SizeBytes { get; set; }
}

public interface IFileStorageService
{
    Task<FileSaveResult> SaveAsync(Stream fileStream, string originalFileName, string contentType,
        UploadPurpose purpose, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
