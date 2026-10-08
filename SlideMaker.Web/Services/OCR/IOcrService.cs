using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.OCR;

public interface IOcrService
{
    Task<OcrResultDto> ExtractTextAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
