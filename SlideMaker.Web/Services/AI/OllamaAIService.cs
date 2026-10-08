using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Options;

namespace SlideMaker.Web.Services.AI;

/// <summary>Free, fully local LLM provider via a running Ollama server (https://ollama.com).</summary>
public class OllamaAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaAIService> _logger;

    public string ProviderName => "Ollama";

    public OllamaAIService(HttpClient httpClient, IOptions<AIOptions> options, ILogger<OllamaAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Ollama;
        _logger = logger;
    }

    public async Task<AIResponse> GenerateContentAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                model = _options.Model,
                prompt = string.IsNullOrWhiteSpace(request.SystemInstruction)
                    ? request.Prompt
                    : $"{request.SystemInstruction}\n\n{request.Prompt}",
                stream = false
            };

            using var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Endpoint.TrimEnd('/')}/api/generate", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Ollama request failed with {StatusCode}: {Body}", response.StatusCode, body);
                return new AIResponse { Success = false, ErrorMessage = "The local Ollama server did not return a successful response." };
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var content = document.RootElement.TryGetProperty("response", out var responseProp)
                ? responseProp.GetString()
                : null;

            var questions = AIJsonResponseParser.Parse(content);
            return new AIResponse { Success = true, RawContent = content, Questions = questions };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not reach the local Ollama server at {Endpoint}", _options.Endpoint);
            return new AIResponse
            {
                Success = false,
                ErrorMessage = $"Could not reach the local Ollama server at {_options.Endpoint}. Is it running?"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Ollama");
            return new AIResponse { Success = false, ErrorMessage = "Unexpected error while generating content with Ollama." };
        }
    }
}
