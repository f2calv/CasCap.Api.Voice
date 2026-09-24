using System.Diagnostics.Metrics;

namespace CasCap.Tests.Unit;

/// <summary>Builds voice-owned metrics for unit tests without application configuration.</summary>
internal static class TestMetrics
{
    /// <summary>Creates a <see cref="VoiceTranscriptionMetrics"/> backed by a throwaway meter factory.</summary>
    public static VoiceTranscriptionMetrics Voice()
    {
        var provider = new ServiceCollection().AddMetrics().BuildServiceProvider();
        return new(
            Options.Create(new VoiceMetricsConfig { MetricNamePrefix = "voice-test" }),
            Options.Create(new SpeechToTextConfig()),
            provider.GetRequiredService<IMeterFactory>());
    }
}