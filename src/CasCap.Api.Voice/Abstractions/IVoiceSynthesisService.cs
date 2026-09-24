namespace CasCap.Abstractions;

/// <summary>Applies spoken-reply policy and synthesizes transport-neutral audio.</summary>
public interface IVoiceSynthesisService
{
    /// <summary>Synthesizes a response when the configured policy permits it.</summary>
    /// <param name="text">Response text to speak.</param>
    /// <param name="inboundWasVoice">Whether the triggering request contained voice audio.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Encoded audio and metadata, or <see langword="null"/> when synthesis is skipped or fails.</returns>
    Task<VoiceSynthesisResult?> TrySynthesizeAsync(
        string? text,
        bool inboundWasVoice,
        CancellationToken cancellationToken = default);
}
