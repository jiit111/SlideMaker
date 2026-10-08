using Microsoft.Extensions.Options;
using SlideMaker.Web.Models;
using SlideMaker.Web.Options;

namespace SlideMaker.Web.Services.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<FileStorageOptions> options, IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task<FileSaveResult> SaveAsync(Stream fileStream, string originalFileName, string contentType,
        UploadPurpose purpose, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (!IsExtensionAllowed(extension, purpose))
        {
            return new FileSaveResult { Success = false, ErrorMessage = $"File type '{extension}' is not allowed for this upload." };
        }

        var maxBytes = (long)_options.MaxFileSizeMB * 1024 * 1024;
        if (fileStream.Length > maxBytes)
        {
            return new FileSaveResult { Success = false, ErrorMessage = $"File exceeds the maximum allowed size of {_options.MaxFileSizeMB} MB." };
        }

        var category = PurposeFolder(purpose);
        var rootPath = Path.Combine(_environment.ContentRootPath, _options.RootPath, category);
        Directory.CreateDirectory(rootPath);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(rootPath, safeFileName);

        try
        {
            await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            fileStream.Position = 0;
            await fileStream.CopyToAsync(output, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save uploaded file for purpose {Purpose}", purpose);
            return new FileSaveResult { Success = false, ErrorMessage = "Could not save the uploaded file." };
        }

        return new FileSaveResult
        {
            Success = true,
            StoredFileName = safeFileName,
            StoragePath = fullPath,
            RelativeUrl = $"/uploads/{category}/{safeFileName}",
            SizeBytes = fileStream.Length
        };
    }

    public Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file at {Path}", storagePath);
            return Task.FromResult(false);
        }
    }

    private bool IsExtensionAllowed(string extension, UploadPurpose purpose) => purpose switch
    {
        UploadPurpose.QuestionImage => _options.AllowedImageExtensions.Contains(extension),
        UploadPurpose.QuestionPdf => extension == ".pdf",
        UploadPurpose.QuestionDocx => extension == ".docx",
        UploadPurpose.TemplateFile => _options.AllowedTemplateExtensions.Contains(extension),
        UploadPurpose.TemplatePreview => _options.AllowedImageExtensions.Contains(extension),
        UploadPurpose.PresentationAsset => _options.AllowedImageExtensions.Contains(extension),
        _ => false
    };

    private static string PurposeFolder(UploadPurpose purpose) => purpose switch
    {
        UploadPurpose.QuestionImage => "images",
        UploadPurpose.QuestionPdf => "documents",
        UploadPurpose.QuestionDocx => "documents",
        UploadPurpose.TemplateFile => "templates",
        UploadPurpose.TemplatePreview => "templates",
        UploadPurpose.PresentationAsset => "presentations",
        _ => "misc"
    };
}
