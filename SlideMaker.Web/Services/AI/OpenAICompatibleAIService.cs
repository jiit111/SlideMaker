using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SlideMaker.Web.DTOs;
using SlideMaker.Web.Options;

namespace SlideMaker.Web.Services.AI;

/// <summary>
/// Generic client for any OpenAI-compatible chat-completions endpoint. Works with paid
/// providers as well as free-tier OpenAI-compatible endpoints (e.g. Groq, OpenRouter) or a
/// self-hosted OpenAI-compatible gateway — endpoint/key/model are entirely config-driven,
/// never hardcoded, and the key never reaches the browser.
/// </summary>
public class OpenAICompatibleAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAICompatibleOptions _options;
    private readonly ILogger<OpenAICompatibleAIService> _logger;

    public string ProviderName => "OpenAICompatible";

    public OpenAICompatibleAIService(HttpClient httpClient, IOptions<AIOptions> options,
        ILogger<OpenAICompatibleAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.OpenAICompatible;
        _logger = logger;
    }

    public async Task<AIResponse> GenerateContentAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AIResponse
            {
                Success = false,
                ErrorMessage = "No OpenAI-compatible endpoint/API key configured. Set AI:OpenAICompatible in appsettings, or switch AI:Provider to \"Mock\"."
            };
        }

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemInstruction))
        {
            messages.Add(new { role = "system", content = request.SystemInstruction });
        }
        messages.Add(new { role = "user", content = request.Prompt });

        // Free-tier / shared-capacity endpoints (e.g. Gemini's free tier) occasionally return a
        // transient 503 "high demand" for a specific model — retry with backoff, then fall
        // through to any configured fallback models before giving up entirely.
        var modelsToTry = new[] { _options.Model }.Concat(_options.FallbackModels).ToArray();
        const int attemptsPerModel = 2;
        HttpStatusCode? lastStatus = null;
        string? lastErrorBody = null;

        foreach (var model in modelsToTry)
        {
            var payload = new { model, messages, temperature = 0.7 };

            for (var attempt = 1; attempt <= attemptsPerModel; attempt++)
            {
                try
                {
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
                    {
                        Content = JsonContent.Create(payload)
                    };
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                    using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        lastStatus = response.StatusCode;
                        lastErrorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning("OpenAI-compatible request with model {Model} failed with {StatusCode} (attempt {Attempt}/{MaxAttempts}): {Body}",
                            model, response.StatusCode, attempt, attemptsPerModel, lastErrorBody);

                        if (IsTransient(response.StatusCode) && attempt < attemptsPerModel)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
                            continue;
                        }

                        if (IsTransient(response.StatusCode))
                        {
                            break; // try the next fallback model, if any
                        }

                        return new AIResponse { Success = false, ErrorMessage = "The AI provider did not return a successful response." };
                    }

                    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                    var content = document.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    var questions = AIJsonResponseParser.Parse(content);
                    return new AIResponse { Success = true, RawContent = content, Questions = questions };
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Unexpected error calling OpenAI-compatible endpoint with model {Model} (attempt {Attempt}/{MaxAttempts})",
                        model, attempt, attemptsPerModel);
                    if (attempt >= attemptsPerModel)
                    {
                        return new AIResponse { Success = false, ErrorMessage = "Unexpected error while generating content from the AI provider." };
                    }
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
                }
            }
        }

        _logger.LogWarning("OpenAI-compatible request exhausted retries across all configured models. Last status: {Status}, body: {Body}", lastStatus, lastErrorBody);
        return new AIResponse { Success = false, ErrorMessage = "The AI provider is temporarily overloaded (high demand). Please try again in a minute." };
    }

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests
            or HttpStatusCode.BadGateway or HttpStatusCode.GatewayTimeout;
}
