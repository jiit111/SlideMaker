using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.AI;

/// <summary>
/// Zero-config, zero-cost AI provider. Returns realistic, well-formed structured questions
/// so the full generation -> validation -> review -> slide pipeline works immediately,
/// without any external API key or local model server.
/// </summary>
public class MockAIService : IAIService
{
    public string ProviderName => "Mock";

    public Task<AIResponse> GenerateContentAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var count = request.MaxQuestions is > 0 ? request.MaxQuestions.Value : 5;
        var questions = new List<AIGeneratedQuestionDto>();

        for (var i = 1; i <= count; i++)
        {
            questions.Add(new AIGeneratedQuestionDto
            {
                Question = $"Sample generated question {i}: What is the correct answer to demonstration item {i}?",
                Options = new List<QuestionOptionDto>
                {
                    new() { Label = "A", Text = $"Option A for item {i}" },
                    new() { Label = "B", Text = $"Option B for item {i}" },
                    new() { Label = "C", Text = $"Option C for item {i}" },
                    new() { Label = "D", Text = $"Option D for item {i}" }
                },
                CorrectAnswer = "B",
                Explanation = $"Option B is correct because this is placeholder sample content for item {i}. Configure a real AI provider in appsettings.json for actual generation.",
                Difficulty = "Medium",
                QuestionType = "MCQ"
            });
        }

        return Task.FromResult(new AIResponse
        {
            Success = true,
            Questions = questions,
            RawContent = "(mock provider — no raw AI content)"
        });
    }
}
