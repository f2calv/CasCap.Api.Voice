namespace CasCap.Fakes;

//ITextToSpeechClient is published as experimental (MEAI001); see AzureSpeechTextToSpeechClient.
#pragma warning disable MEAI001

/// <summary>An <see cref="ITextToSpeechClient"/> returning canned audio, so voice consumers can be tested without a backend.</summary>
public sealed class FakeTextToSpeechClient : ITextToSpeechClient
{
    /// <summary>The audio returned by <see cref="GetAudioAsync"/>.</summary>
    /// <remarks>Defaults to the four-byte <c>OggS</c> container signature.</remarks>
    public byte[] Audio { get; set; } = [0x4F, 0x67, 0x67, 0x53];

    /// <summary>The media type reported for <see cref="Audio"/>.</summary>
    /// <remarks>Defaults to <see cref="AzureSpeechTextToSpeechClient.OggOpusMediaType"/>.</remarks>
    public string MediaType { get; set; } = AzureSpeechTextToSpeechClient.OggOpusMediaType;

    /// <summary>When set, <see cref="GetAudioAsync"/> fails with this instead of returning.</summary>
    public Exception? Failure { get; set; }

    /// <summary>The number of synthesis requests received.</summary>
    public int Requests { get; private set; }

    /// <summary>The text of the most recent request, so a test can assert what was spoken.</summary>
    public string? LastText { get; private set; }

    /// <inheritdoc/>
    public Task<TextToSpeechResponse> GetAudioAsync(string text, TextToSpeechOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Requests++;
        LastText = text;
        if (Failure is not null)
            return Task.FromException<TextToSpeechResponse>(Failure);
        return Task.FromResult(new TextToSpeechResponse([new DataContent(Audio, MediaType)]));
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<TextToSpeechResponseUpdate> GetStreamingAudioAsync(string text,
        TextToSpeechOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetAudioAsync(text, options, cancellationToken);
        foreach (var update in response.ToTextToSpeechResponseUpdates())
            yield return update;
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    /// <inheritdoc/>
    public void Dispose() { }
}
