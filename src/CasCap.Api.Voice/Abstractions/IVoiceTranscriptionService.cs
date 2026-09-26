namespace CasCap.Abstractions;

/// <summary>Validates, normalizes, and transcribes encoded voice audio.</summary>
public interface IVoiceTranscriptionService
{
    /// <summary>Processes one encoded audio payload through the configured speech-to-text provider.</summary>
    /// <param name="audio">Encoded audio bytes. The implementation does not mutate the array.</param>
    /// <param name="mediaType">Declared audio MIME type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A typed terminal outcome containing transcript text only on success.</returns>
    Task<VoiceTranscriptionResult> Transcribe(
        byte[] audio,
        string mediaType,
        CancellationToken cancellationToken = default);
}
