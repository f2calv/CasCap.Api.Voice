namespace CasCap.Tests.Unit;

#pragma warning disable MEAI001

/// <summary>Verifies validation, normalisation and admission policy for voice transcription.</summary>
[Trait("Category", "SpeechToText")]
public sealed class VoiceMessageTranscriptionServiceTests
{
    [Theory]
    [InlineData("audio/basic")]
    [InlineData("image/png")]
    [InlineData("")]
    public async Task Transcribe_UnsupportedMediaTypeDoesNotCallBackend(string mediaType)
    {
        var client = new FakeSpeechToTextClient();
        using var service = CreateService(client);

        var result = await service.TranscribeAsync(CreateWav(), mediaType, TestContext.Current.CancellationToken);

        Assert.Equal(VoiceTranscriptionOutcome.Unsupported, result.Outcome);
        Assert.Equal(0, client.Requests);
    }

    [Fact]
    public async Task Transcribe_EmptyPayloadIsInvalid()
    {
        var client = new FakeSpeechToTextClient();
        using var service = CreateService(client);

        var result = await service.TranscribeAsync([], "audio/wav", TestContext.Current.CancellationToken);

        Assert.Equal(VoiceTranscriptionOutcome.Invalid, result.Outcome);
        Assert.Equal(0, client.Requests);
    }

    [Fact]
    public async Task Transcribe_NormalisesTranscriptAndRecordsDuration()
    {
        var client = new FakeSpeechToTextClient { Transcript = "  hello   there  " };
        using var service = CreateService(client);

        var result = await service.TranscribeAsync(CreateWav(), "audio/wav", TestContext.Current.CancellationToken);

        Assert.Equal(VoiceTranscriptionOutcome.Success, result.Outcome);
        Assert.Equal("hello there", result.Text);
        Assert.Equal(1, client.Requests);
        Assert.NotNull(result.AudioDuration);
        Assert.NotNull(result.TranscriptionDuration);
        Assert.Null(result.TranscodeDuration);
    }

    [Fact]
    public async Task Transcribe_EmptyTranscriptIsRejected()
    {
        var client = new FakeSpeechToTextClient { Transcript = "   " };
        using var service = CreateService(client);

        var result = await service.TranscribeAsync(CreateWav(), "audio/wav", TestContext.Current.CancellationToken);

        Assert.Equal(VoiceTranscriptionOutcome.EmptyTranscript, result.Outcome);
    }

    [Fact]
    public async Task Transcribe_BackendFailureIsContained()
    {
        var client = new FakeSpeechToTextClient { Failure = new HttpRequestException("backend failed") };
        using var service = CreateService(client);

        var result = await service.TranscribeAsync(CreateWav(), "audio/wav", TestContext.Current.CancellationToken);

        Assert.Equal(VoiceTranscriptionOutcome.BackendFailed, result.Outcome);
    }

    [Fact]
    public async Task Transcribe_RejectsOverlongAudio()
    {
        var client = new FakeSpeechToTextClient();
        using var service = CreateService(client, new() { MaxDurationSeconds = 1 });

        var result = await service.TranscribeAsync(CreateWav(seconds: 2), "audio/wav",
            TestContext.Current.CancellationToken);

        Assert.Equal(VoiceTranscriptionOutcome.Oversized, result.Outcome);
        Assert.Equal(0, client.Requests);
    }

    private static VoiceMessageTranscriptionService CreateService(FakeSpeechToTextClient client,
        SpeechToTextConfig? config = null) =>
        new(NullLogger<VoiceMessageTranscriptionService>.Instance,
            Options.Create(config ?? new SpeechToTextConfig()), client,
            TestMetrics.Voice("voice-transcription-service-test"));

    private static byte[] CreateWav(double seconds = 1)
    {
        const int sampleRate = 16_000;
        const short channels = 1;
        const short bitsPerSample = 16;
        var dataLength = (int)(sampleRate * channels * (bitsPerSample / 8) * seconds);
        using var buffer = new MemoryStream();
        using var writer = new BinaryWriter(buffer, Encoding.ASCII, leaveOpen: true);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataLength);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * (bitsPerSample / 8));
        writer.Write((short)(channels * (bitsPerSample / 8)));
        writer.Write(bitsPerSample);
        writer.Write("data"u8);
        writer.Write(dataLength);
        writer.Write(new byte[dataLength]);
        writer.Flush();
        return buffer.ToArray();
    }
}
