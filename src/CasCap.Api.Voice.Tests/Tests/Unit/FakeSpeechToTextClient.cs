using System.Runtime.CompilerServices;

namespace CasCap.Tests.Unit;

#pragma warning disable MEAI001

/// <summary>A canned speech-to-text client for voice policy tests.</summary>
public sealed class FakeSpeechToTextClient : ISpeechToTextClient
{
    /// <summary>The transcript returned by the fake.</summary>
    public string Transcript { get; set; } = "transcribed text";

    /// <summary>An exception to throw instead of returning a response.</summary>
    public Exception? Failure { get; set; }

    /// <summary>The number of calls received.</summary>
    public int Requests { get; private set; }

    /// <inheritdoc/>
    public Task<SpeechToTextResponse> GetTextAsync(Stream audioSpeechStream,
        SpeechToTextOptions? options = null, CancellationToken cancellationToken = default)
    {
        Requests++;
        if (Failure is not null)
            return Task.FromException<SpeechToTextResponse>(Failure);
        return Task.FromResult(new SpeechToTextResponse(Transcript));
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<SpeechToTextResponseUpdate> GetStreamingTextAsync(Stream audioSpeechStream,
        SpeechToTextOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetTextAsync(audioSpeechStream, options, cancellationToken);
        foreach (var update in response.ToSpeechToTextResponseUpdates())
            yield return update;
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    /// <inheritdoc/>
    public void Dispose() { }
}

