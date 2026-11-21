using Azure.Core;
using Azure.Identity;

namespace CasCap.Tests;

/// <summary>Provides local-only configuration and Azure CLI authentication for the Realtime probe.</summary>
public abstract class TestBase(ITestOutputHelper output)
{
    protected ITestOutputHelper Output { get; } = output;

    internal const string LocalSettingsFileName = "appsettings.Local.json";

    internal static AzureRealtimeProbeConfig LoadProbeConfig(string basePath)
    {
        using var configuration = new ConfigurationManager();
        configuration.SetBasePath(basePath)
            .AddJsonFile(LocalSettingsFileName, optional: true, reloadOnChange: false);
        return configuration.GetSection(AzureRealtimeProbeConfig.ConfigurationSectionName)
            .Get<AzureRealtimeProbeConfig>() ?? new();
    }

    /// <summary>Gets a Microsoft Entra access token from the active Azure CLI session.</summary>
    protected static async ValueTask<AccessToken> GetAzureCliTokenAsync(string scope, CancellationToken cancellationToken)
    {
        var credential = new AzureCliCredential();
        try
        {
            return await credential.GetTokenAsync(new TokenRequestContext([scope]), cancellationToken);
        }
        catch (AuthenticationFailedException) when (!cancellationToken.IsCancellationRequested)
        {
            throw Xunit.Sdk.FailException.ForFailure(
                "Could not get a Microsoft Entra token from Azure CLI. Run az login in the Azure resource's tenant.");
        }
    }
}
