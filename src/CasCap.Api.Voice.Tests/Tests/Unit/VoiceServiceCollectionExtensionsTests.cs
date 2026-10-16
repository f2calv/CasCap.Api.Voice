namespace CasCap.Tests.Unit;

/// <summary>Verifies the replaceable Voice pipeline registrations.</summary>
public sealed class VoiceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSpeechToText_RegistersTranscriptionAbstraction()
    {
        var services = new ServiceCollection();

        services.AddSpeechToText();

        var descriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IVoiceTranscriptionService));
        Assert.Equal(typeof(VoiceMessageTranscriptionService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddSpeechToText_PreservesCustomTranscriptionService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IVoiceTranscriptionService, CustomTranscriptionService>();

        services.AddSpeechToText();

        var descriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IVoiceTranscriptionService));
        Assert.Equal(typeof(CustomTranscriptionService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddTextToSpeech_RegistersSynthesisAbstraction()
    {
        var services = new ServiceCollection();

        services.AddTextToSpeech();

        var descriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IVoiceSynthesisService));
        Assert.Equal(typeof(VoiceReplySynthesisService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddTextToSpeech_PreservesCustomSynthesisService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IVoiceSynthesisService, CustomSynthesisService>();

        services.AddTextToSpeech();

        var descriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IVoiceSynthesisService));
        Assert.Equal(typeof(CustomSynthesisService), descriptor.ImplementationType);
    }

    [Fact]
    public async Task AddTextToSpeech_DisabledModeNeedsNoProviderConfiguration()
    {
        //The default provider is Azure AI Speech, which otherwise demands an endpoint and credential on resolution.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        services.AddTextToSpeech();
        await using var provider = services.BuildServiceProvider();

        var result = await provider.GetRequiredService<IVoiceSynthesisService>()
            .TrySynthesizeAsync("hallo", inboundWasVoice: true, TestContext.Current.CancellationToken);

        Assert.Equal(VoiceSynthesisOutcome.Skipped, result.Outcome);
    }

    private sealed class CustomTranscriptionService : IVoiceTranscriptionService
    {
        public Task<VoiceTranscriptionResult> Transcribe(
            byte[] audio,
            string mediaType,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(VoiceTranscriptionResult.Failure(VoiceTranscriptionOutcome.BackendFailed));
    }

    private sealed class CustomSynthesisService : IVoiceSynthesisService
    {
        public Task<VoiceSynthesisResult> TrySynthesizeAsync(
            string? text,
            bool inboundWasVoice,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(VoiceSynthesisResult.Skipped);
    }
}
