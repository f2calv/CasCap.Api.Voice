namespace CasCap.Models;

/// <summary>The typed outcome of a spoken-reply synthesis attempt.</summary>
/// <remarks>
/// <see cref="Audio"/>, <see cref="MediaType"/> and <see cref="FileName"/> are populated only when
/// <see cref="Outcome"/> is <see cref="VoiceSynthesisOutcome.Success"/>.
/// </remarks>
public sealed record VoiceSynthesisResult
{
    /// <inheritdoc cref="VoiceSynthesisOutcome" path="/summary"/>
    public required VoiceSynthesisOutcome Outcome { get; init; }

    /// <summary>The encoded audio bytes.</summary>
    public ReadOnlyMemory<byte> Audio { get; init; }

    /// <summary>The audio MIME type.</summary>
    public string? MediaType { get; init; }

    /// <summary>The safe filename offered to a transport adapter.</summary>
    public string? FileName { get; init; }

    /// <summary>The configured provider that was attempted, or <see langword="null"/> when policy skipped synthesis.</summary>
    public TextToSpeechProvider? Provider { get; init; }

    /// <summary>The exception type and message of a backend failure; never the reply text.</summary>
    public string? Error { get; init; }

    /// <summary>Whether audio was produced.</summary>
    public bool AudioAvailable => Outcome is VoiceSynthesisOutcome.Success;

    /// <summary>Whether synthesis was attempted and did not produce audio, as opposed to being skipped by policy.</summary>
    public bool Failed => Outcome is not (VoiceSynthesisOutcome.Success or VoiceSynthesisOutcome.Skipped);

    /// <summary>The result used when policy declines to speak the reply.</summary>
    public static VoiceSynthesisResult Skipped { get; } = new() { Outcome = VoiceSynthesisOutcome.Skipped };

    /// <summary>Creates a successful result carrying encoded audio.</summary>
    /// <param name="audio">The encoded audio bytes.</param>
    /// <param name="mediaType">The audio MIME type.</param>
    /// <param name="fileName">The safe filename offered to a transport adapter.</param>
    /// <param name="provider">The provider that produced the audio.</param>
    /// <returns>A successful <see cref="VoiceSynthesisResult"/>.</returns>
    public static VoiceSynthesisResult Success(ReadOnlyMemory<byte> audio, string mediaType, string fileName,
        TextToSpeechProvider provider) =>
        new()
        {
            Outcome = VoiceSynthesisOutcome.Success,
            Audio = audio,
            MediaType = mediaType,
            FileName = fileName,
            Provider = provider,
        };

    /// <summary>Creates a result for an attempted synthesis that produced no audio.</summary>
    /// <param name="outcome">The failure outcome.</param>
    /// <param name="provider">The provider that was attempted.</param>
    /// <param name="error">The exception type and message, when an exception occurred.</param>
    /// <returns>A non-success <see cref="VoiceSynthesisResult"/> carrying no audio.</returns>
    public static VoiceSynthesisResult Failure(VoiceSynthesisOutcome outcome, TextToSpeechProvider provider,
        string? error = null) =>
        new() { Outcome = outcome, Provider = provider, Error = error };
}
