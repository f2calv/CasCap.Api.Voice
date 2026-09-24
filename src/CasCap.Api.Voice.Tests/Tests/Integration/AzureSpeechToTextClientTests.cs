namespace CasCap.Tests;

#pragma warning disable MEAI001

/// <summary>Optional live Azure Speech adapter coverage.</summary>
[Trait("Category", "SpeechToText")]
[Trait("Category", "Integration")]
public sealed class AzureSpeechToTextClientTests(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void TranscribeAsync_RequiresExplicitProviderConfiguration()
    {
        var config = Configuration.GetSection(SpeechToTextConfig.ConfigurationSectionName)
            .Get<SpeechToTextConfig>();
        Assert.SkipWhen(string.IsNullOrWhiteSpace(config?.AzureEndpoint),
            $"{nameof(SpeechToTextConfig.AzureEndpoint)} is not configured here.");
        Assert.Skip("Live Azure Speech execution is intentionally disabled in the credential-free suite.");
    }
}
