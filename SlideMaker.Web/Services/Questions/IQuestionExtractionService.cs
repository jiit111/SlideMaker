using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.Questions;

/// <summary>
/// Parses raw OCR/PDF/DOCX text into structured questions. Kept separate from the source
/// (image/PDF/DOCX) so every extraction path shares the same question-boundary logic.
/// </summary>
public interface IQuestionExtractionService
{
    List<ExtractedQuestionDto> ParseQuestions(string rawText);
}
