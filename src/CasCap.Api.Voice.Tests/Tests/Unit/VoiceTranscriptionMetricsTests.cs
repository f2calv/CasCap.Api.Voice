using System.Diagnostics.Metrics;

namespace CasCap.Tests.Unit;

/// <summary>Verifies voice metrics and their bounded dimensions.</summary>
[Trait("Category", "SpeechToText")]
public sealed class VoiceTranscriptionMetricsTests
{
    [Fact]
    public void Record_SuccessPublishesEveryStage()
    {
        var measurements = Collect(m => m.Record(VoiceTranscriptionResult.Success("hello",
            TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))));

        Assert.Equal(4, Value(measurements, "voice-test.voice.audio.duration"));
        Assert.Equal(2, Value(measurements, "voice-test.voice.transcode.duration"));
        Assert.Equal(1, Value(measurements, "voice-test.voice.transcription.duration"));
        Assert.Equal(2, Value(measurements, "voice-test.voice.transcode.speed"));
        Assert.Equal(4, Value(measurements, "voice-test.voice.transcription.speed"));
        Assert.Equal(1, Value(measurements, "voice-test.voice.transcriptions"));
    }

    [Fact]
    public void Record_FailureStillCountsOutcomeAndProvider()
    {
        var measurements = Collect(m => m.Record(VoiceTranscriptionResult.Failure(
            VoiceTranscriptionOutcome.BackendFailed)));
        var counter = Assert.Single(measurements, x => x.Instrument == "voice-test.voice.transcriptions");

        Assert.Equal(nameof(VoiceTranscriptionOutcome.BackendFailed),
            Assert.Single(counter.Tags, t => t.Key == VoiceTranscriptionMetrics.OutcomeTagName).Value);
        Assert.Equal(nameof(SpeechToTextProvider.WhisperAsr),
            Assert.Single(counter.Tags, t => t.Key == VoiceTranscriptionMetrics.ProviderTagName).Value);
        Assert.Single(measurements);
    }

    [Fact]
    public void Record_SkippedTranscodeIsNotReported()
    {
        var measurements = Collect(m => m.Record(VoiceTranscriptionResult.Success("hello",
            audioDuration: TimeSpan.FromSeconds(4), transcriptionDuration: TimeSpan.FromSeconds(1))));

        Assert.DoesNotContain(measurements, x => x.Instrument.StartsWith("voice-test.voice.transcode",
            StringComparison.Ordinal));
    }

    private static double Value(List<Measured> measurements, string instrument) =>
        Assert.Single(measurements, x => x.Instrument == instrument).Value;

    private static List<Measured> Collect(Action<VoiceTranscriptionMetrics> action)
    {
        var measurements = new List<Measured>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Name.StartsWith("voice-test.voice.", StringComparison.Ordinal))
                    l.EnableMeasurementEvents(instrument);
            },
        };
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            measurements.Add(new(instrument.Name, value, tags.ToArray())));
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            measurements.Add(new(instrument.Name, value, tags.ToArray())));
        listener.Start();
        action(TestMetrics.Voice());
        listener.RecordObservableInstruments();
        return measurements;
    }

    private sealed record Measured(string Instrument, double Value, KeyValuePair<string, object?>[] Tags);
}