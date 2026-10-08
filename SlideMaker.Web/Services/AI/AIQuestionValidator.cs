using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.AI;

/// <summary>
/// AI output must never be trusted blindly (spec §39-40): validates option count,
/// correct-answer consistency, and requested-vs-returned question count before
/// anything reaches the question bank.
/// </summary>
public static class AIQuestionValidator
{
    public static AIValidationResult Validate(List<AIGeneratedQuestionDto> questions, int? expectedCount)
    {
        var errors = new List<string>();

        if (questions.Count == 0)
        {
            errors.Add("The AI response did not contain any questions.");
        }

        if (expectedCount.HasValue && questions.Count != expectedCount.Value)
        {
            errors.Add($"Requested {expectedCount.Value} question(s) but received {questions.Count}.");
        }

        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var label = $"Question {i + 1}";

            if (string.IsNullOrWhiteSpace(q.Question))
            {
                errors.Add($"{label}: question text is missing.");
                continue;
            }

            var isMcq = string.IsNullOrEmpty(q.QuestionType) ||
                        q.QuestionType.Equals("MCQ", StringComparison.OrdinalIgnoreCase) ||
                        q.QuestionType.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase);

            if (isMcq)
            {
                if (q.Options.Count != 4)
                {
                    errors.Add($"{label}: expected exactly 4 options, found {q.Options.Count}.");
                }

                if (string.IsNullOrWhiteSpace(q.CorrectAnswer))
                {
                    errors.Add($"{label}: correct answer is missing.");
                }
                else
                {
                    var matches = q.Options.Any(o =>
                        string.Equals(o.Label, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(o.Text, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase));
                    if (!matches)
                    {
                        errors.Add($"{label}: correct answer '{q.CorrectAnswer}' does not match any option.");
                    }
                }
            }
        }

        return new AIValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
