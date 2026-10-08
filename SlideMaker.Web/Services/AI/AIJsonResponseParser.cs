using System.Text.Json;
using System.Text.Json.Serialization;
using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.AI;

/// <summary>Deserializes AI JSON responses into strongly-typed DTOs instead of fragile string splitting.</summary>
public static class AIJsonResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static List<AIGeneratedQuestionDto> Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new List<AIGeneratedQuestionDto>();
        }

        var cleaned = StripMarkdownFences(content);
        var start = cleaned.IndexOf('{');
        var end = cleaned.LastIndexOf('}');
        if (start < 0 || end < start)
        {
            return new List<AIGeneratedQuestionDto>();
        }

        var jsonSlice = cleaned.Substring(start, end - start + 1);

        try
        {
            var wrapper = JsonSerializer.Deserialize<AIQuestionsWrapper>(jsonSlice, JsonOptions);
            return wrapper?.Questions ?? new List<AIGeneratedQuestionDto>();
        }
        catch (JsonException)
        {
            return new List<AIGeneratedQuestionDto>();
        }
    }

    private static string StripMarkdownFences(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0)
            {
                trimmed = trimmed[(firstNewline + 1)..];
            }
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0)
            {
                trimmed = trimmed[..lastFence];
            }
        }
        return trimmed;
    }

    private class AIQuestionsWrapper
    {
        [JsonPropertyName("questions")]
        public List<AIGeneratedQuestionDto> Questions { get; set; } = new();
    }
}
