using System.Runtime.CompilerServices;

namespace CasCap.Tests.Unit;

#pragma warning disable MEAI001

/// <summary>A canned text-to-speech client for voice policy tests.</summary>
public sealed class FakeTextToSpeechClient : ITextToSpeechClient
{
    /// <summary>The audio returned by the fake.</summary>
    public byte[] Audio { get; set; } = [0x4F, 0x67, 0x67, 0x53];

    /// <summary>The media type returned by the fake.</summary>
    public string MediaType { get; set; } = AzureSpeechTextToSpeechClient.OggOpusMediaType;

    /// <summary>An exception to throw instead of returning a response.</summary>
    public Exception? Failure { get; set; }

    /// <summary>The number of calls received.</summary>
    public int Requests { get; private set; }

    /// <summary>The most recent text sent to the fake.</summary>
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
