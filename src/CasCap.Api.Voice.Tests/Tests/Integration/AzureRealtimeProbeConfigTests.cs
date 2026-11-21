namespace CasCap.Tests;

[Trait("Category", "Integration")]
public sealed class AzureRealtimeProbeConfigTests(ITestOutputHelper output) : TestBase(output), IDisposable
{
    private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory();

    [Fact]
    public void LoadProbeConfig_IgnoresStandardSettingsWithoutLocalOptIn()
    {
        WriteSettings("appsettings.Test.json", enabled: true);
        Assert.False(LoadProbeConfig(_directory.FullName).Enabled);

        WriteSettings(LocalSettingsFileName, enabled: false);
        Assert.False(LoadProbeConfig(_directory.FullName).Enabled);
    }

    [Fact]
    public void LoadProbeConfig_BindsOnlyLocalValues()
    {
        WriteSettings("appsettings.Test.json", enabled: false);
        WriteSettings(LocalSettingsFileName, enabled: true);

        var settings = LoadProbeConfig(_directory.FullName);

        Assert.True(settings.Enabled);
        Assert.Equal(new Uri("https://example.com/"), settings.GetResourceEndpoint());
        Assert.Equal("example-deployment", settings.Deployment);
    }

    [Theory]
    [InlineData("http://example.com/", "example-deployment", nameof(AzureRealtimeProbeConfig.Endpoint))]
    [InlineData("https://example.com/openai", "example-deployment", nameof(AzureRealtimeProbeConfig.Endpoint))]
    [InlineData("https://example.com/", "", nameof(AzureRealtimeProbeConfig.Deployment))]
    public void GetResourceEndpoint_IdentifiesInvalidOptedInSetting(string endpoint, string deployment, string property)
    {
        var path = Path.Combine(_directory.FullName, LocalSettingsFileName);
        File.WriteAllText(path, $$"""
            { "AzureRealtimeProbe": { "Enabled": true, "Endpoint": "{{endpoint}}", "Deployment": "{{deployment}}" } }
            """);
        var settings = LoadProbeConfig(_directory.FullName);

        var exception = Assert.Throws<OptionsValidationException>(() => settings.GetResourceEndpoint());

        Assert.Contains($"{AzureRealtimeProbeConfig.ConfigurationSectionName}:{property}", exception.Message);
        Assert.Contains(LocalSettingsFileName, exception.Message);
        Assert.DoesNotContain(endpoint, exception.Message);
    }

    public void Dispose() => _directory.Delete(recursive: true);

    private void WriteSettings(string fileName, bool enabled)
    {
        File.WriteAllText(Path.Combine(_directory.FullName, fileName), $$"""
            { "AzureRealtimeProbe": { "Enabled": {{(enabled ? "true" : "false")}},
            "Endpoint": "https://example.com/", "Deployment": "example-deployment" } }
            """);
    }
}
