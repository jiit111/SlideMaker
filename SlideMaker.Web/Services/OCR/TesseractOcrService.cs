using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Options;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Options;
using Tesseract;

namespace SlideMaker.Web.Services.OCR;

/// <summary>
/// Wraps the local/free Tesseract OCR engine. Requires eng.traineddata / hin.traineddata
/// to be present under the configured TessDataPath (not bundled by the NuGet package).
/// Fails gracefully with a friendly error instead of crashing the request pipeline.
/// </summary>
public class TesseractOcrService : IOcrService
{
    private readonly OcrOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(IOptions<OcrOptions> options, IWebHostEnvironment environment,
        ILogger<TesseractOcrService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public Task<OcrResultDto> ExtractTextAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        var tessDataPath = Path.IsPathRooted(_options.TessDataPath)
            ? _options.TessDataPath
            : Path.Combine(_environment.ContentRootPath, _options.TessDataPath);

        if (!Directory.Exists(tessDataPath) || Directory.GetFiles(tessDataPath, "*.traineddata").Length == 0)
        {
            _logger.LogWarning("Tesseract trained data not found at {Path}", tessDataPath);
            return Task.FromResult(new OcrResultDto
            {
                Success = false,
                ErrorMessage = "OCR language data is not installed on this server. Please add eng.traineddata / hin.traineddata under " +
                               $"'{_options.TessDataPath}', or enter the questions manually below."
            });
        }

        try
        {
            using var memoryStream = new MemoryStream();
            imageStream.Position = 0;
            imageStream.CopyTo(memoryStream);

            var normalizedBytes = NormalizeExifOrientation(memoryStream.ToArray());

            using var engine = new TesseractEngine(tessDataPath, _options.Languages, EngineMode.Default);
            using var img = Pix.LoadFromMemory(normalizedBytes);
            using var page = engine.Process(img);

            var text = page.GetText();
            return Task.FromResult(new OcrResultDto { Success = true, RawText = text });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tesseract OCR extraction failed");
            return Task.FromResult(new OcrResultDto
            {
                Success = false,
                ErrorMessage = "Could not read text from the uploaded image. Please try a clearer image or enter questions manually."
            });
        }
    }

    /// <summary>
    /// Phone photos routinely carry an EXIF orientation tag (rotated/mirrored) that most
    /// viewers apply automatically but Tesseract's image loader (Leptonica) does not — it
    /// reads the raw, un-rotated pixels. Left uncorrected, this silently turns a perfectly
    /// upright photo into OCR garbage. This bakes the correct orientation into the pixels
    /// before Tesseract ever sees them.
    /// </summary>
    private static byte[] NormalizeExifOrientation(byte[] imageBytes)
    {
        const int orientationPropertyId = 0x0112;

        using var inputStream = new MemoryStream(imageBytes);
        using var image = Image.FromStream(inputStream);

        if (Array.IndexOf(image.PropertyIdList, orientationPropertyId) < 0)
        {
            return imageBytes;
        }

        var orientation = image.GetPropertyItem(orientationPropertyId)!.Value![0];
        var flip = orientation switch
        {
            2 => RotateFlipType.RotateNoneFlipX,
            3 => RotateFlipType.Rotate180FlipNone,
            4 => RotateFlipType.Rotate180FlipX,
            5 => RotateFlipType.Rotate90FlipX,
            6 => RotateFlipType.Rotate90FlipNone,
            7 => RotateFlipType.Rotate270FlipX,
            8 => RotateFlipType.Rotate270FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone
        };

        if (flip == RotateFlipType.RotateNoneFlipNone)
        {
            return imageBytes;
        }

        image.RotateFlip(flip);
        image.RemovePropertyItem(orientationPropertyId);

        using var outputStream = new MemoryStream();
        image.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
        return outputStream.ToArray();
    }
}
