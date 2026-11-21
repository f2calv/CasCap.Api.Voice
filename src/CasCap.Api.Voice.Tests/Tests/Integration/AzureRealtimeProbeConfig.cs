namespace CasCap.Tests;

internal sealed class AzureRealtimeProbeConfig
{
    public const string ConfigurationSectionName = "AzureRealtimeProbe";

    public bool Enabled { get; init; }

    public string Endpoint { get; init; } = string.Empty;

    public string Deployment { get; init; } = string.Empty;

    public Uri GetResourceEndpoint()
    {
        if (string.IsNullOrWhiteSpace(Deployment))
            throw InvalidSetting(nameof(Deployment), "Set it to the deployment name, not the model name.");

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var resource) ||
            resource.Scheme != Uri.UriSchemeHttps || resource.PathAndQuery != "/" ||
            resource.UserInfo.Length != 0 || resource.Fragment.Length != 0)
            throw InvalidSetting(nameof(Endpoint), "Set it to the HTTPS resource root.");

        return resource;
    }

    private static OptionsValidationException InvalidSetting(string property, string guidance) =>
        new(ConfigurationSectionName, typeof(AzureRealtimeProbeConfig),
            [$"Invalid {ConfigurationSectionName}:{property} in {TestBase.LocalSettingsFileName}. {guidance}"]);
}
