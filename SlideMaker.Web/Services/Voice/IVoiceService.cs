namespace SlideMaker.Web.Services.Voice;

/// <summary>
/// Architecture placeholder for server-side speech-to-text (spec §19). Phase 1 uses the
/// browser's built-in Web Speech API client-side (see wwwroot/js/voice-input.js) so no
/// implementation is registered yet; a server-side provider can be added later without
/// touching any controller or view.
/// </summary>
public interface IVoiceService
{
    Task<string> ConvertSpeechToTextAsync(Stream audioStream, CancellationToken cancellationToken = default);
}
