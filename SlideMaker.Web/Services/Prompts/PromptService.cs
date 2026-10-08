using System.Text;
using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.Prompts;

public class PromptService : IPromptService
{
    public string BuildQuestionGenerationPrompt(AIQuestionGenerationRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Generate {request.NumberOfQuestions} {request.QuestionType} question(s) for the following context:");
        sb.AppendLine($"Subject: {request.Subject}");
        if (!string.IsNullOrWhiteSpace(request.ClassLevel)) sb.AppendLine($"Class/Level: {request.ClassLevel}");
        if (!string.IsNullOrWhiteSpace(request.Chapter)) sb.AppendLine($"Chapter: {request.Chapter}");
        if (!string.IsNullOrWhiteSpace(request.Topic)) sb.AppendLine($"Topic: {request.Topic}");
        sb.AppendLine($"Difficulty: {request.Difficulty}");
        sb.AppendLine($"Language: {request.Language}");
        if (!string.IsNullOrWhiteSpace(request.AdditionalInstruction))
        {
            sb.AppendLine($"Additional instruction: {request.AdditionalInstruction}");
        }
        sb.AppendLine();
        sb.AppendLine("Base every question strictly on the standard textbook/curriculum content of the named chapter/topic for that subject and class level " +
                       "(e.g. NCERT or the equivalent standard curriculum) — the facts, definitions, options and correct answer must be factually accurate " +
                       "and specific to that chapter, not generic or made up.");
        sb.AppendLine("Each question must have exactly 4 options (A-D), one correct answer matching an option, and a short explanation.");
        sb.AppendLine(JsonResponseInstruction());
        return sb.ToString();
    }

    public string BuildFreeformGenerationPrompt(string requirementText)
    {
        var sb = new StringBuilder();
        sb.AppendLine("The user described what they want in their own words (may mix Hindi and English):");
        sb.AppendLine($"\"{requirementText}\"");
        sb.AppendLine();
        sb.AppendLine("Interpret the subject, chapter/topic, number of questions, difficulty and language from this text, and generate matching multiple-choice questions.");
        sb.AppendLine("Each question must have exactly 4 options (A-D), one correct answer matching an option, and a short explanation.");
        sb.AppendLine(JsonResponseInstruction());
        return sb.ToString();
    }

    public string BuildExtractionRepairPrompt(string rawOcrText)
    {
        var sb = new StringBuilder();
        sb.AppendLine("The following text was extracted via OCR from a photo of exam questions and may contain recognition errors:");
        sb.AppendLine(rawOcrText);
        sb.AppendLine();
        sb.AppendLine("Reconstruct the individual questions, options, answers and explanations as accurately as possible, preserving the original language (Hindi/English/mixed).");
        sb.AppendLine(JsonResponseInstruction());
        return sb.ToString();
    }

    public string JsonResponseInstruction() =>
        """
        Respond with ONLY valid JSON in this exact shape, no markdown fences, no extra commentary:
        {
          "questions": [
            {
              "question": "...",
              "options": [ { "label": "A", "text": "..." }, { "label": "B", "text": "..." }, { "label": "C", "text": "..." }, { "label": "D", "text": "..." } ],
              "correctAnswer": "B",
              "explanation": "...",
              "difficulty": "Easy|Medium|Hard",
              "topic": "...",
              "chapter": "...",
              "questionType": "MCQ"
            }
          ]
        }
        """;
}
