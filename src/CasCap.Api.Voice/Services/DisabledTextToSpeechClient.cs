namespace CasCap.Services;

#pragma warning disable MEAI001

/// <summary>Stands in for the configured provider while spoken replies are disabled.</summary>
/// <remarks>
/// Lets a host run with <see cref="VoiceReplyMode.Disabled"/> without the provider's endpoint or
/// credential; <see cref="VoiceReplySynthesisService"/> never calls it in that mode.
/// </remarks>
internal sealed class DisabledTextToSpeechClient : ITextToSpeechClient
{
    /// <inheritdoc/>
    public Task<TextToSpeechResponse> GetAudioAsync(string text, TextToSpeechOptions? options = null,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"Text-to-speech is disabled by {nameof(TextToSpeechConfig)}.{nameof(TextToSpeechConfig.Mode)}.");

    /// <inheritdoc/>
    public IAsyncEnumerable<TextToSpeechResponseUpdate> GetStreamingAudioAsync(string text,
        TextToSpeechOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException($"Text-to-speech is disabled by {nameof(TextToSpeechConfig)}.{nameof(TextToSpeechConfig.Mode)}.");

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <inheritdoc/>
    public void Dispose() { }
}
