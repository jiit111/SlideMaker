namespace SlideMaker.Web.DTOs;

public class ExtractedQuestionDto
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<QuestionOptionDto> Options { get; set; } = new();
    public string? Answer { get; set; }
    public string? Explanation { get; set; }
}

public class QuestionOptionDto
{
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class OcrResultDto
{
    public string RawText { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
