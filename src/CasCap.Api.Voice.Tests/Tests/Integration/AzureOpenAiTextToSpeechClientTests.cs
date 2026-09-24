namespace CasCap.Tests;

#pragma warning disable MEAI001

/// <summary>Optional live Azure OpenAI synthesis adapter coverage.</summary>
[Trait("Category", "TextToSpeech")]
[Trait("Category", "Integration")]
public sealed class AzureOpenAiTextToSpeechClientTests(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void GetAudioAsync_RequiresExplicitProviderConfiguration()
    {
        var config = Configuration.GetSection(TextToSpeechConfig.ConfigurationSectionName)
            .Get<TextToSpeechConfig>();
        Assert.SkipWhen(string.IsNullOrWhiteSpace(config?.AzureOpenAiEndpoint),
            $"{nameof(TextToSpeechConfig.AzureOpenAiEndpoint)} is not configured here.");
        Assert.Skip("Live Azure OpenAI execution is intentionally disabled in the credential-free suite.");
    }
}
