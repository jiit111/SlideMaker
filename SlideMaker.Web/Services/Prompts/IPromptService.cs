using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.Prompts;

/// <summary>Builds AI prompts from templates so wording lives here, not scattered across controllers.</summary>
public interface IPromptService
{
    string BuildQuestionGenerationPrompt(AIQuestionGenerationRequest request);
    string BuildFreeformGenerationPrompt(string requirementText);
    string BuildExtractionRepairPrompt(string rawOcrText);
    string JsonResponseInstruction();
}
