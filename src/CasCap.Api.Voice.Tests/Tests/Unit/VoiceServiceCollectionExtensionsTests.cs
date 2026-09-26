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
        public Task<VoiceSynthesisResult?> TrySynthesizeAsync(
            string? text,
            bool inboundWasVoice,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<VoiceSynthesisResult?>(null);
    }
}
