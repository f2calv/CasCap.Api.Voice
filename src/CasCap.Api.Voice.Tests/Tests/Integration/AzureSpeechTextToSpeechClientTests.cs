namespace CasCap.Tests;

#pragma warning disable MEAI001

/// <summary>Optional live Azure Speech synthesis adapter coverage.</summary>
[Trait("Category", "TextToSpeech")]
[Trait("Category", "Integration")]
public sealed class AzureSpeechTextToSpeechClientTests(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void GetAudioAsync_RequiresExplicitProviderConfiguration()
    {
        var config = Configuration.GetSection(TextToSpeechConfig.ConfigurationSectionName)
            .Get<TextToSpeechConfig>();
        Assert.SkipWhen(string.IsNullOrWhiteSpace(config?.AzureSpeechEndpoint),
            $"{nameof(TextToSpeechConfig.AzureSpeechEndpoint)} is not configured here.");
        Assert.Skip("Live Azure Speech execution is intentionally disabled in the credential-free suite.");
    }
}

