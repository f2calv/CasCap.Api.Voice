namespace CasCap.Models;

/// <summary>Configuration for the metrics emitted by the reusable voice pipeline.</summary>
public sealed record VoiceMetricsConfig : IAppConfig
{
    /// <inheritdoc/>
    public static string ConfigurationSectionName => $"{nameof(CasCap)}:{nameof(VoiceMetricsConfig)}";

    /// <summary>Prefix used for the voice meter and instrument names.</summary>
    /// <remarks>Defaults to the library-neutral <c>voice</c> prefix.</remarks>
    [Required, MinLength(1)]
    public string MetricNamePrefix { get; init; } = "voice";
}
