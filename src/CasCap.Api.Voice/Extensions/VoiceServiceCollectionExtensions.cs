namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Extension methods for registering the reusable voice processing pipeline.</summary>
public static class VoiceServiceCollectionExtensions
{
    /// <summary>Registers speech-to-text configuration, providers, metrics, and the transcription policy.</summary>
    /// <param name="services">The service collection.</param>
    public static void AddSpeechToText(this IServiceCollection services)
    {
        services.AddCasCapConfiguration<SpeechToTextConfig>();
        services.AddCasCapConfiguration<VoiceMetricsConfig>();
        services.AddHttpClient(WhisperAsrSpeechToTextClient.HttpClientName, (sp, client) =>
        {
            client.Timeout = TimeSpan.FromMilliseconds(
                sp.GetRequiredService<IOptions<SpeechToTextConfig>>().Value.TimeoutMs);
        });
        services.AddHttpClient(WhisperCppSpeechToTextClient.HttpClientName, (sp, client) =>
        {
            client.Timeout = TimeSpan.FromMilliseconds(
                sp.GetRequiredService<IOptions<SpeechToTextConfig>>().Value.TimeoutMs);
        });
        services.AddAzureSpeechService();
#pragma warning disable MEAI001
        services.TryAddSingleton<ISpeechToTextClient>(sp =>
            sp.GetRequiredService<IOptions<SpeechToTextConfig>>().Value.Provider switch
            {
                SpeechToTextProvider.WhisperCpp => ActivatorUtilities.CreateInstance<WhisperCppSpeechToTextClient>(sp),
                SpeechToTextProvider.Azure => ActivatorUtilities.CreateInstance<AzureSpeechToTextClient>(sp),
                _ => ActivatorUtilities.CreateInstance<WhisperAsrSpeechToTextClient>(sp),
            });
#pragma warning restore MEAI001
        services.TryAddSingleton<VoiceTranscriptionMetrics>();
        services.TryAddSingleton<VoiceMessageTranscriptionService>();
    }

    /// <summary>Registers text-to-speech configuration, providers, and the synthesis policy.</summary>
    /// <param name="services">The service collection.</param>
    public static void AddTextToSpeech(this IServiceCollection services)
    {
        services.AddCasCapConfiguration<TextToSpeechConfig>();
        services.AddAzureSpeechService();
        services.AddHttpClient(AzureOpenAiTextToSpeechClient.HttpClientName, (sp, client) =>
        {
            client.Timeout = TimeSpan.FromMilliseconds(
                sp.GetRequiredService<IOptions<TextToSpeechConfig>>().Value.TimeoutMs);
        });
#pragma warning disable MEAI001
        services.TryAddSingleton<ITextToSpeechClient>(sp =>
        {
            var provider = sp.GetRequiredService<IOptions<TextToSpeechConfig>>().Value.Provider;
            return provider switch
            {
                TextToSpeechProvider.AzureOpenAi =>
                    ActivatorUtilities.CreateInstance<AzureOpenAiTextToSpeechClient>(sp),
                TextToSpeechProvider.Piper =>
                    ActivatorUtilities.CreateInstance<PiperTextToSpeechClient>(sp),
                TextToSpeechProvider.AzureSpeechContainer
                    or TextToSpeechProvider.Kokoro or TextToSpeechProvider.CoquiXtts =>
                    throw new NotImplementedException(
                        $"{nameof(TextToSpeechProvider)}.{provider} is not implemented; see the remarks on that member."),
                _ => ActivatorUtilities.CreateInstance<AzureSpeechTextToSpeechClient>(sp),
            };
        });
#pragma warning restore MEAI001
        services.TryAddSingleton<VoiceReplySynthesisService>();
    }

    private static void AddAzureSpeechService(this IServiceCollection services)
    {
        services.TryAddSingleton<ISpeechService>(sp =>
        {
            var endpoint = sp.GetRequiredService<IOptions<SpeechToTextConfig>>().Value.AzureEndpoint
                ?? sp.GetRequiredService<IOptions<TextToSpeechConfig>>().Value.AzureSpeechEndpoint
                ?? throw new InvalidOperationException(
                    $"{nameof(SpeechToTextConfig)}.{nameof(SpeechToTextConfig.AzureEndpoint)} or " +
                    $"{nameof(TextToSpeechConfig)}.{nameof(TextToSpeechConfig.AzureSpeechEndpoint)} is required when " +
                    "an Azure AI Speech provider is selected.");
            var credential = sp.GetRequiredService<IOptions<AzureAuthConfig>>().Value.TokenCredential
                ?? throw new InvalidOperationException(
                    $"{nameof(AzureAuthConfig)}.{nameof(AzureAuthConfig.TokenCredential)} is required when " +
                    "an Azure AI Speech provider is selected.");
            return new SpeechService(new Uri(endpoint), credential);
        });
    }
}